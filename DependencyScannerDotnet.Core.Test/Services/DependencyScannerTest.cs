using DependencyScannerDotnet.Core.Model;
using DependencyScannerDotnet.Core.Services;
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

        public class XUnitProjectSourceMock : ProjectSource
        {
            public XUnitProjectSourceMock() : base() { }

            public override Task<List<ProjectReference>> LoadProjectFilesAsync()
            {
                ProjectReference projectReference = new()
                {
                    ProjectName = "Dummy.Proj",
                    Version = "0.0.1",
                    IsNewSdkStyle = true
                };

                projectReference.Targets.Add("net5.0");

                projectReference.PackageReferences.Add(
                    new PackageReference
                    {
                        PackageId = "xunit",
                        Version = "2.4.1"
                    }
                );

                return Task.FromResult(new List<ProjectReference>() { projectReference });
            }
        }
    }
}
