using DependencyScannerDotnet.Core.Model;
using DependencyScannerDotnet.Core.Services;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DependencyScannerDotnet.Core.Test.Services
{
    public class DependencyScannerTest
    {
        public DependencyScannerTest() { }

        [Fact]
        public async Task ScanDependenciesAsync_Ok()
        {
            XUnitProjectSourceMock projectSource = new();
            TargetFrameworkMappingService targetFrameworkMappingService = new();
            PackageUpgrader packageUpgrader = new();
            DependencyScanner dependencyScanner = new(projectSource, targetFrameworkMappingService, packageUpgrader);

            ScanResult result = await dependencyScanner.ScanDependenciesAsync(new());

            Assert.NotNull(result);

            List<ProjectReference> projects = result.Projects;

            Assert.NotNull(projects);
            Assert.Single(projects);

            ProjectReference project = projects[0];

            Assert.Equal("Dummy.Proj", project.ProjectName);
            Assert.Equal("0.0.1", project.Version);
            Assert.True(project.IsNewSdkStyle);
            Assert.True(project.ProjectReferences == null || !project.ProjectReferences.Any());
            Assert.NotNull(project.PackageReferences);
            Assert.Single(project.PackageReferences);
            Assert.Equal("xunit", project.PackageReferences[0].PackageId);
            Assert.Equal("2.4.1", project.PackageReferences[0].Version);
            Assert.Collection(
                project.PackageReferences[0].PackageReferences,
                package => Assert.True(package.PackageId == "xunit.analyzers" && package.Version == "0.10.0" && (package.PackageReferences == null || !package.PackageReferences.Any())),
                package => Assert.True(package.PackageId == "xunit.assert" && package.Version == "2.4.1" && package.PackageReferences != null && package.PackageReferences.Count == 1),
                package => Assert.True(package.PackageId == "xunit.core" && package.Version == "2.4.1" && package.PackageReferences != null && package.PackageReferences.Count == 2)
            );
        }

        [Fact]
        public async Task ScanDependenciesAsync_UnknownReferencedProject()
        {
            UnknownReferenceProjectSourceMock projectSource = new();
            TargetFrameworkMappingService targetFrameworkMappingService = new();
            PackageUpgrader packageUpgrader = new();
            DependencyScanner dependencyScanner = new(projectSource, targetFrameworkMappingService, packageUpgrader);

            ScanResult result = await dependencyScanner.ScanDependenciesAsync(new());

            Assert.NotNull(result);
            Assert.NotNull(result.Projects);
            Assert.Collection(
                result.Projects,
                project => Assert.True(project.ProjectName == "Dummy.Proj" && project.FullFileName == @"C:\temp\Dummy.Proj\Dummy.Proj.csproj")
            );
        }

        [Fact]
        public void FindPackageVersionConflicts_Ok()
        {
            ProjectReference projectA = new() { ProjectName = "ProjectA", PackageReferences = new() { new() { PackageId = "test.package", Version = "1.2.0" }, new() { PackageId = "other.package", Version = "1.0.0" } } };
            ProjectReference projectB = new() { ProjectName = "ProjectB", ProjectReferences = new() { projectA }, PackageReferences = new() { new() { PackageId = "test.package", Version = "2.0.0" } } };

            List<ProjectReference> projects = new() { projectA, projectB };

            ScanResult scanResult = new(projects, null, null);

            DependencyScanner dependencyScanner = new(null, null, null);

            dependencyScanner.FindPackageVersionConflicts(scanResult, new());

            Assert.True(projectA.PackageReferences[0].HasPotentialVersionConflict);
            Assert.False(projectA.PackageReferences[1].HasPotentialVersionConflict);
            Assert.True(projectB.PackageReferences[0].HasPotentialVersionConflict);
            Assert.NotNull(scanResult.ConflictPackages);
            Assert.Collection(
                scanResult.ConflictPackages,
                conflictPackage => Assert.True(conflictPackage.PackageId == "test.package" && conflictPackage.Versions != null && conflictPackage.Versions[0] == "1.2.0" && conflictPackage.Versions[1] == "2.0.0")
            );
            Assert.NotNull(scanResult.ConflictPackages[0].Projects);
            Assert.Collection(
                scanResult.ConflictPackages[0].Projects,
                project => Assert.Equal(projectA, project),
                project => Assert.Equal(projectB, project)
            );
        }

        [Fact]
        public void SearchPackagesInProjects_Ok()
        {
            PackageReference nuGetFrameworks5100 = new() { PackageId = "NuGet.Frameworks", Version = "5.10.0" };
            PackageReference nuGetCommon5100 = new() { PackageId = "NuGet.Common", Version = "5.10.0", PackageReferences = new() { nuGetFrameworks5100 } };
            PackageReference xunitCore = new() { PackageId = "xunit.core", Version = "2.4.1" };

            ProjectReference dummyProjA = new() { ProjectName = "Dummy.Proj.A", Version = "1.0.0", PackageReferences = new() { nuGetFrameworks5100 } };
            ProjectReference dummyProjB = new() { ProjectName = "Dummy.Proj.B", Version = "1.0.0", PackageReferences = new() { nuGetCommon5100, xunitCore } };

            ScanResult scanResult = new(new() { dummyProjA, dummyProjB }, null, null);

            DependencyScanner dependencyScanner = new(null, null, null);

            List<PackageWithReferencingProjects> result = dependencyScanner.SearchPackagesInProjects(scanResult, "uget");

            Assert.NotNull(result);
            Assert.Collection(
                result,
                package =>
                {
                    Assert.Equal("NuGet.Common", package.Package.PackageId);
                    Assert.Equal("5.10.0", package.Package.Version);
                    Assert.NotNull(package.Projects);
                    Assert.Collection(
                        package.Projects,
                        project => Assert.Equal(dummyProjB, project)
                    );
                },
                package =>
                {
                    Assert.Equal("NuGet.Frameworks", package.Package.PackageId);
                    Assert.Equal("5.10.0", package.Package.Version);
                    Assert.NotNull(package.Projects);
                    Assert.Collection(
                        package.Projects,
                        project => Assert.Equal(dummyProjA, project),
                        project => Assert.Equal(dummyProjB, project)
                    );
                }
            );
        }

        public class XUnitProjectSourceMock : ProjectSource
        {
            public XUnitProjectSourceMock() : base() { }

            public override Task<List<ProjectFile>> LoadProjectFilesAsync()
            {
                ProjectFile projectFile = new()
                {
                    ProjectName = "Dummy.Proj",
                    Version = "0.0.1",
                    IsNewSdkStyle = true,
                    FullFileName = @"C:\temp\Project.Lib\Dummy.Proj.csproj"
                };

                projectFile.Targets.Add("net5.0");

                projectFile.ReferencedPackages.Add(new("xunit", new NuGetVersion("2.4.1")));

                return Task.FromResult(new List<ProjectFile>() { projectFile });
            }
        }

        public class UnknownReferenceProjectSourceMock : ProjectSource
        {
            public UnknownReferenceProjectSourceMock() : base() { }

            public override Task<List<ProjectFile>> LoadProjectFilesAsync()
            {
                ProjectFile projectFile = new()
                {
                    ProjectName = "Dummy.Proj",
                    Version = "0.0.1",
                    IsNewSdkStyle = true,
                    FullFileName = @"C:\temp\Dummy.Proj\Dummy.Proj.csproj"
                };

                ProjectFile unknownProjectFile = new()
                {
                    ProjectName = "Unknown.Proj",
                    FullFileName = @"C:\temp\Unknown.Proj\Unknown.Proj.csproj"
                };

                projectFile.Targets.Add("net5.0");

                projectFile.ReferencedProjects.Add(unknownProjectFile);

                return Task.FromResult(new List<ProjectFile>() { projectFile });
            }
        }
    }
}
