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
            List<ProjectReference> projectReferences = await m_projectSource.LoadProjectFilesAsync().ConfigureAwait(false);

            CancellationToken cancellationToken = CancellationToken.None;

            using SourceCacheContext cache = new();

            SourceRepository repository = Repository.Factory.GetCoreV3(GetPackageSourceStr(), FeedType.FileSystemV3);

            foreach (ProjectReference projectReference in projectReferences)
            {
                NuGetFramework framework = null;

                if (projectReference.Targets != null && projectReference.Targets.Any())
                {
                    framework = NuGetFramework.ParseFolder(m_targetFrameworkMappingService.GetNugetFolderForTargetFramework(projectReference.Targets.First()));
                }

                framework ??= NuGetFramework.AnyFramework;

                foreach (PackageReference packageReference in projectReference.PackageReferences)
                {
                    await ScanDependenciesAsync(packageReference, cache, repository, framework, 0, maxDepth, cancellationToken).ConfigureAwait(false);
                }
            }

            ScanResult scanResult = new(projectReferences, null);

            FindPackageVersionConflicts(scanResult);

            return scanResult;
        }

        private async Task ScanDependenciesAsync(PackageReference packageReference, SourceCacheContext cache, SourceRepository repository, NuGetFramework framework, int depth, int maxDepth, CancellationToken cancellationToken)
        {
            if (depth >= maxDepth)
            {
                return;
            }

            DependencyInfoResource dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancellationToken).ConfigureAwait(false);

            PackageIdentity packageIdentity = new(packageReference.PackageId, new NuGetVersion(packageReference.Version));
            SourcePackageDependencyInfo dependencyInfo = await dependencyInfoResource.ResolvePackage(packageIdentity, framework, cache, NullLogger.Instance, cancellationToken).ConfigureAwait(false);

            if (dependencyInfo != null)
            {
                foreach (PackageDependency packageDependency in dependencyInfo.Dependencies)
                {
                    PackageReference childPackageReference = new()
                    {
                        PackageId = packageDependency.Id,
                        Version = packageDependency.VersionRange.MinVersion.ToString()
                    };

                    packageReference.PackageReferences.Add(childPackageReference);

                    await ScanDependenciesAsync(childPackageReference, cache, repository, framework, depth + 1, maxDepth, cancellationToken).ConfigureAwait(false);
                }

                if (packageReference.PackageReferences != null && packageReference.PackageReferences.Any())
                {
                    packageReference.PackageReferences = packageReference.PackageReferences
                        .OrderBy(package => package.PackageId.ToLower())
                        .ToList();
                }
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
                .GroupBy(package => package.PackageId)
                .Where(group => group.ToList().Select(package => package.Version).ToHashSet().Count > 1)
                .ToList();

            conflictPackages.ForEach(group =>
            {
                List<PackageReference> packages = group.ToList();
                packages.ForEach(package => package.HasPotentialVersionConflict = true);

                List<string> versions = packages.Select(package => package.Version)
                    .ToHashSet()
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
