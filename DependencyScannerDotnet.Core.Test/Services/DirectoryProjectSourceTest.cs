using DependencyScannerDotnet.Core.Model;
using DependencyScannerDotnet.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DependencyScannerDotnet.Core.Test.Services
{
    public class DirectoryProjectSourceTest
    {
        public DirectoryProjectSourceTest() { }

        [Fact]
        public async Task LoadProjectFilesAsync_Ok()
        {
            int hierarchyUp = 3;
            DirectoryInfo directoryInfo = new(new FileInfo(GetType().Assembly.Location).DirectoryName);

            for (int i = 0; i < hierarchyUp; i++)
            {
                directoryInfo = directoryInfo.Parent;
            }

            DirectoryProjectSource projectSource = new(directoryInfo.FullName);
            List<ProjectFile> projects = await projectSource.LoadProjectFilesAsync();

            Assert.NotNull(projects);
            Assert.NotEmpty(projects);

            ProjectFile testProject = projects.Where(project => project.ProjectName == "DependencyScannerDotnet.Core.Test").SingleOrDefault();

            Assert.NotNull(testProject);
            Assert.Equal(new FileInfo(Path.Combine(directoryInfo.FullName, "DependencyScannerDotnet.Core.Test.csproj")).FullName, testProject.FullFileName);
            Assert.True(testProject.IsNewSdkStyle);
            Assert.NotNull(testProject.Targets);
            Assert.Collection(
                testProject.Targets,
                target => Assert.Equal("net5.0", target)
            );
            Assert.NotNull(testProject.ReferencedProjects);
            Assert.Collection(
                testProject.ReferencedProjects,
                project => Assert.True(project.ProjectName == "DependencyScannerDotnet.Core"
                                            && project.FullFileName == new DirectoryInfo(Path.Combine(directoryInfo.FullName, @"..\DependencyScannerDotnet.Core\DependencyScannerDotnet.Core.csproj")).FullName)
            );
            Assert.NotNull(testProject.ReferencedPackages);
            Assert.Equal(4, testProject.ReferencedPackages.Count);
            Assert.Contains(testProject.ReferencedPackages, package => package.Id == "xunit" && package.Version.ToString() == "2.4.1");
        }
    }
}
