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
    public class ProjectSourceTest
    {
        public ProjectSourceTest() { }

        [Fact]
        public async Task LoadProjectFilesAsync_Ok()
        {
            XUnitProjectSourceMock projectSource = new();
            List<ProjectFile> projects = await projectSource.LoadProjectFilesAsync();

            Assert.NotNull(projects);
            Assert.NotEmpty(projects);

            ProjectFile testProject = projects.Where(project => project.ProjectName == "Dummy.Test.Lib").SingleOrDefault();

            Assert.NotNull(testProject);
            Assert.True(testProject.IsNewSdkStyle);
            Assert.NotNull(testProject.Targets);
            Assert.Collection(
                testProject.Targets,
                target => Assert.Equal("net5.0", target)
            );
            Assert.NotNull(testProject.ReferencedProjects);
            Assert.Collection(
                testProject.ReferencedProjects,
                project => Assert.Equal("Dummy.Test.App", project.ProjectName)
            );
            Assert.NotNull(testProject.ReferencedPackages);
            Assert.Collection(
                testProject.ReferencedPackages,
                package => Assert.True(package.Id == "xunit" && package.Version.ToString() == "2.4.1")
            );
        }

        [Fact]
        public async Task LoadProjectFilesAsync_Legacy_Ok()
        {
            XUnitLegacyProjectSourceMock projectSource = new();
            List<ProjectFile> projects = await projectSource.LoadProjectFilesAsync();

            Assert.NotNull(projects);
            Assert.NotEmpty(projects);

            ProjectFile testProject = projects.Where(project => project.ProjectName == "Dummy.Test.Lib").SingleOrDefault();

            Assert.NotNull(testProject);
            Assert.False(testProject.IsNewSdkStyle);
            Assert.NotNull(testProject.Targets);
            Assert.Collection(
                testProject.Targets,
                target => Assert.Equal("net472", target)
            );
            Assert.NotNull(testProject.ReferencedProjects);
            Assert.Collection(
                testProject.ReferencedProjects,
                project => Assert.Equal("Dummy.Test.App", project.ProjectName)
            );
            Assert.NotNull(testProject.ReferencedPackages);
            Assert.Collection(
                testProject.ReferencedPackages,
                package => Assert.True(package.Id == "xunit" && package.Version.ToString() == "2.4.1")
            );
        }

        public class XUnitProjectSourceMock : ProjectSource
        {
            public XUnitProjectSourceMock() : base() { }

            public override Task<List<ProjectFile>> LoadProjectFilesAsync()
            {
                string csprojStr = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <Version>0.1.0</Version>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""xunit"" Version=""2.4.1"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\Dummy.Test.App\Dummy.Test.App.csproj"" />
  </ItemGroup>
</Project>";

                ProjectFile projectFile = ParseProjectFile(Encoding.UTF8.GetBytes(csprojStr));
                projectFile.ProjectName = "Dummy.Test.Lib";

                return Task.FromResult(new List<ProjectFile>() { projectFile });
            }
        }

        public class XUnitLegacyProjectSourceMock : ProjectSource
        {
            public XUnitLegacyProjectSourceMock() : base() { }

            public override Task<List<ProjectFile>> LoadProjectFilesAsync()
            {
                string csprojStr = @"<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include=""Dummy.cs"" />
  </ItemGroup>
<ItemGroup>
    <ProjectReference Include=""..\Dummy.Test.App\Dummy.Test.App.csproj"">
      <Project>{349da30c-0dca-4b8f-8d66-91a4f21dc2f6}</Project>
      <Name>Dummy.Test.App</Name>
    </ProjectReference>
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include=""xunit"">
      <Version>2.4.1</Version>
    </PackageReference>
  </ItemGroup>
</Project>";

                ProjectFile projectFile = ParseProjectFile(Encoding.UTF8.GetBytes(csprojStr));
                projectFile.ProjectName = "Dummy.Test.Lib";

                return Task.FromResult(new List<ProjectFile>() { projectFile });
            }
        }
    }
}
