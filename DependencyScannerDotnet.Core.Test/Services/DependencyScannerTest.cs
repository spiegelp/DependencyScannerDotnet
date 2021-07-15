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
            DependencyScanner dependencyScanner = new(projectSource, targetFrameworkMappingService);

            ScanResult result = await dependencyScanner.ScanDependenciesAsync();

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
        public void FindPackageVersionConflicts_Ok()
        {
            ProjectReference projectA = new() { ProjectName = "ProjectA", PackageReferences = new() { new() { PackageId = "test.package", Version = "1.2.0" }, new() { PackageId = "other.package", Version = "1.0.0" } } };
            ProjectReference projectB = new() { ProjectName = "ProjectB", ProjectReferences = new() { projectA }, PackageReferences = new() { new() { PackageId = "test.package", Version = "2.0.0" } } };

            List<ProjectReference> projects = new() { projectA, projectB };

            ScanResult scanResult = new(projects, null);

            DependencyScanner dependencyScanner = new(null, null);

            dependencyScanner.FindPackageVersionConflicts(scanResult);

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

        public class XUnitProjectSourceMock : ProjectSource
        {
            public XUnitProjectSourceMock() : base() { }

            public override Task<List<ProjectFile>> LoadProjectFilesAsync()
            {
                ProjectFile projectFile = new()
                {
                    ProjectName = "Dummy.Proj",
                    Version = "0.0.1",
                    IsNewSdkStyle = true
                };

                projectFile.Targets.Add("net5.0");

                projectFile.ReferencedPackages.Add(new("xunit", new NuGetVersion("2.4.1")));

                return Task.FromResult(new List<ProjectFile>() { projectFile });
            }
        }
    }
}
