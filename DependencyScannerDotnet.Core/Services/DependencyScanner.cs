using DependencyScannerDotnet.Core.Model;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public class DependencyScanner : IDependencyScanner
    {
        private readonly IProjectSource m_projectSource;
        private readonly ITargetFrameworkMappingService m_targetFrameworkMappingService;

        public DependencyScanner(IProjectSource projectSource, ITargetFrameworkMappingService targetFrameworkMappingService)
        {
            m_projectSource = projectSource;
            m_targetFrameworkMappingService = targetFrameworkMappingService;
        }

        public async Task<ScanResult> ScanDependenciesAsync(int maxDepth = 64)
        {
            List<ProjectFile> projectFiles = await m_projectSource.LoadProjectFilesAsync().ConfigureAwait(false);

            Dictionary<PackageIdentity, PackageReference> packageReferenceCache = new(PackageIdentityComparer.Default);

            Dictionary<string, ProjectReference> projectsByName = projectFiles
                .Select(projectFile => new ProjectReference()
                {
                    ProjectName = projectFile.ProjectName,
                    Version = projectFile.Version,
                    IsNewSdkStyle = projectFile.IsNewSdkStyle,
                    Targets = new(projectFile.Targets)
                })
                .ToDictionary(projectReference => projectReference.ProjectName);

            projectFiles.ForEach(projectFile =>
            {
                if (projectFile.ReferencedProjects != null && projectFile.ReferencedProjects.Any())
                {
                    ProjectReference projectReference = projectsByName[projectFile.ProjectName];

                    projectFile.ReferencedProjects.ForEach(referencedProjectFile => projectReference.ProjectReferences.Add(projectsByName[referencedProjectFile.ProjectName]));
                }
            });

            CancellationToken cancellationToken = CancellationToken.None;

            using SourceCacheContext cache = new();

            SourceRepository repository = Repository.Factory.GetCoreV3(GetPackageSourceStr(), FeedType.FileSystemV3);

            foreach (ProjectFile projectFile in projectFiles)
            {
                NuGetFramework framework = null;

                if (projectFile.Targets != null && projectFile.Targets.Any())
                {
                    framework = NuGetFramework.ParseFolder(m_targetFrameworkMappingService.GetNugetFolderForTargetFramework(projectFile.Targets.First()));
                }

                framework ??= NuGetFramework.AnyFramework;

                foreach (PackageIdentity packageIdentity in projectFile.ReferencedPackages)
                {
                    PackageReference packageReference = await ScanDependenciesAsync(packageIdentity, cache, repository, framework, packageReferenceCache, 0, maxDepth, cancellationToken).ConfigureAwait(false);

                    if (packageReference != null)
                    {
                        projectsByName[projectFile.ProjectName].PackageReferences.Add(packageReference);
                    }
                }
            }

            ScanResult scanResult = new(projectsByName.Values.ToList(), null);

            FindPackageVersionConflicts(scanResult);

            return scanResult;
        }

        private async Task<PackageReference> ScanDependenciesAsync(PackageIdentity packageIdentity, SourceCacheContext cache, SourceRepository repository, NuGetFramework framework,
            Dictionary<PackageIdentity, PackageReference> packageReferenceCache, int depth, int maxDepth, CancellationToken cancellationToken)
        {
            if (packageReferenceCache.TryGetValue(packageIdentity, out PackageReference packageReference))
            {
                return packageReference;
            }

            if (depth >= maxDepth)
            {
                return null;
            }

            DependencyInfoResource dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancellationToken).ConfigureAwait(false);

            SourcePackageDependencyInfo dependencyInfo = await dependencyInfoResource.ResolvePackage(packageIdentity, framework, cache, NullLogger.Instance, cancellationToken).ConfigureAwait(false);

            if (dependencyInfo != null)
            {
                packageReference = new()
                {
                    PackageId = dependencyInfo.Id,
                    Version = dependencyInfo.Version.ToString()
                };

                packageReferenceCache[packageIdentity] = packageReference;

                foreach (PackageDependency packageDependency in dependencyInfo.Dependencies)
                {
                    PackageIdentity childPackageIdentity = new(packageDependency.Id, packageDependency.VersionRange.MinVersion);

                    PackageReference childPackageReference = await ScanDependenciesAsync(childPackageIdentity, cache, repository, framework, packageReferenceCache, depth + 1, maxDepth, cancellationToken).ConfigureAwait(false);

                    if (childPackageReference != null)
                    {
                        packageReference.PackageReferences.Add(childPackageReference);
                    }
                }

                packageReference.PackageReferences = packageReference.PackageReferences
                    .OrderBy(package => package.PackageId.ToLower())
                    .ToList();

                return packageReference;
            }
            else
            {
                return null;
            }
        }

        private string GetPackageSourceStr()
        {
            // https://api.nuget.org/v3/index.json

            string localPackageCache = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

            if (string.IsNullOrWhiteSpace(localPackageCache))
            {
                localPackageCache = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "NuGet", "Cache");
            }

            return localPackageCache;
        }

        public void FindPackageVersionConflicts(ScanResult scanResult)
        {
            scanResult.ConflictPackages = new();

            List<IGrouping<string, PackageReference>> conflictPackages = scanResult.Projects
                .SelectMany(project => project.PackageReferences)
                .Distinct()
                .GroupBy(package => package.PackageId)
                .Where(group => group.ToList().Select(package => package.Version).Distinct().Count() > 1)
                .ToList();

            conflictPackages.ForEach(group =>
            {
                List<PackageReference> packages = group.ToList();
                packages.ForEach(package => package.HasPotentialVersionConflict = true);

                List<string> versions = packages.Select(package => package.Version)
                    .Distinct()
                    .Select(version => new NuGetVersion(version))
                    .OrderBy(version => version)
                    .Select(version => version.ToString())
                    .ToList();

                scanResult.ConflictPackages.Add(new ConflictPackage(group.Key, versions));
            });

            if (scanResult.ConflictPackages.Count > 1)
            {
                scanResult.ConflictPackages = scanResult.ConflictPackages
                    .OrderBy(conflictPackage => conflictPackage.PackageId)
                    .ToList();
            }
        }
    }
}
