using DependencyScannerDotnet.Core.Model;
using DependencyScannerDotnet.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DependencyScannerDotnet.Core.Test.Services
{
    public class PackageUpgraderTest
    {
        public PackageUpgraderTest() { }

        [Fact]
        public void GetProjectsForPackageUpdate_Ok()
        {
            int hierarchyUp = 4;
            DirectoryInfo directoryInfo = new(new FileInfo(GetType().Assembly.Location).DirectoryName);

            for (int i = 0; i < hierarchyUp; i++)
            {
                directoryInfo = directoryInfo.Parent;
            }

            List<ProjectReference> projects = new List<ProjectReference>
            {
                new()
                {
                    ProjectName = "DependencyScannerDotnet.Core",
                    FullFileName = @$"{directoryInfo}\DependencyScannerDotnet.Core\DependencyScannerDotnet.Core.csproj",
                    PackageReferences =  new List<PackageReference>
                    {
                        new() { PackageId = "Newtonsoft.Json" },
                        new() { PackageId = "NuGet.Protocol" }
                    }
                },
                new()
                {
                    ProjectName = "DependencyScannerDotnet.Core.Test",
                    FullFileName = @$"{directoryInfo}\DependencyScannerDotnet.Core.Test\DependencyScannerDotnet.Core.Test",
                    PackageReferences =  new List<PackageReference>
                    {
                        new() { PackageId = "xunit" }
                    }
                }
            };

            PackageUpgrader packageUpgrader = new();
            projects = packageUpgrader.GetProjectsForPackageUpdate(projects, new() { PackageId = "Newtonsoft.Json" });

            Assert.Collection(
                projects,
                x => Assert.Equal("DependencyScannerDotnet.Core", x.ProjectName)
            );
        }

        [Fact]
        public void UpdatePackageVersionNewSdkStyleProject_Ok()
        {
            string csprojStr = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <Version>0.1.0</Version>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""xunit"" Version=""2.3.0"" />
    <PackageReference Include=""NuGet.Protocol"" Version=""5.10.0"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\Dummy.Test.App\Dummy.Test.App.csproj"" />
  </ItemGroup>
</Project>";

            string expectedCsprojStr = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <Version>0.1.0</Version>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""xunit"" Version=""2.4.1"" />
    <PackageReference Include=""NuGet.Protocol"" Version=""5.10.0"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\Dummy.Test.App\Dummy.Test.App.csproj"" />
  </ItemGroup>
</Project>";

            PackageUpgrader packageUpgrader = new();
            string newCsprojStr = packageUpgrader.UpdatePackageVersionNewSdkStyleProject(csprojStr, "xunit", "2.4.1");

            Assert.Equal(expectedCsprojStr, newCsprojStr);
        }

        [Fact]
        public void UpdatePackageVersionLegacyProject_Ok()
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
      <Version>2.3.0</Version>
    </PackageReference>
    <PackageReference Include=""NuGet.Protocol"">
      <Version>5.10.0</Version>
    </PackageReference>
  </ItemGroup>
</Project>";

            string expectedCsprojStr = @"<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
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
    <PackageReference Include=""NuGet.Protocol"">
      <Version>5.10.0</Version>
    </PackageReference>
  </ItemGroup>
</Project>";

            PackageUpgrader packageUpgrader = new();
            string newCsprojStr = packageUpgrader.UpdatePackageVersionLegacyProject(csprojStr, "xunit", "2.4.1");

            Assert.Equal(expectedCsprojStr, newCsprojStr);
        }
    }
}
