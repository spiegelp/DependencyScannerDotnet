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
    public class FileSystemProjectSourceTest
    {
        public FileSystemProjectSourceTest() { }

        [Fact]
        public async Task LoadProjectFilesAsync_Ok()
        {
            int hierarchyUp = 3;
            DirectoryInfo directoryInfo = new(new FileInfo(GetType().Assembly.Location).DirectoryName);

            for (int i = 0; i < hierarchyUp; i++)
            {
                directoryInfo = directoryInfo.Parent;
            }

            FileSystemProjectSource projectSource = new(directoryInfo.FullName);
            List<ProjectFile> projects = await projectSource.LoadProjectFilesAsync();

            Assert.NotNull(projects);
            Assert.NotEmpty(projects);

            ProjectFile testProject = projects.Where(project => project.ProjectName == "DependencyScannerDotnet.Core.Test").SingleOrDefault();

            Assert.NotNull(testProject);
            Assert.True(testProject.IsNewSdkStyle);
            Assert.NotNull(testProject.Targets);
            Assert.Single(testProject.Targets);
            Assert.Contains(testProject.Targets, target => target == "net5.0");
            Assert.NotNull(testProject.ReferencedProjects);
            Assert.Collection(
                testProject.ReferencedProjects,
                project => Assert.Equal("DependencyScannerDotnet.Core", project.ProjectName)
            );
            Assert.NotNull(testProject.ReferencedPackages);
            Assert.Equal(4, testProject.ReferencedPackages.Count);
            Assert.Contains(testProject.ReferencedPackages, package => package.Id == "xunit" && package.Version.ToString() == "2.4.1");
        }
    }
}
