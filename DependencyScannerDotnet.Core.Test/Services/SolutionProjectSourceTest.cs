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
    public class SolutionProjectSourceTest
    {
        public SolutionProjectSourceTest() { }

        [Fact]
        public async Task LoadProjectFilesAsync_Ok()
        {
            int hierarchyUp = 4;
            DirectoryInfo directoryInfo = new(new FileInfo(GetType().Assembly.Location).DirectoryName);

            for (int i = 0; i < hierarchyUp; i++)
            {
                directoryInfo = directoryInfo.Parent;
            }

            string solutionFileName = Path.Combine(directoryInfo.FullName, "DependencyScannerDotnet.sln");

            SolutionProjectSource projectSource = new(solutionFileName);
            List<ProjectFile> projects = await projectSource.LoadProjectFilesAsync();

            Assert.NotNull(projects);
            Assert.NotEmpty(projects);
            Assert.Equal(3, projects.Count);

            ProjectFile coreProject = projects.Where(project => project.ProjectName == "DependencyScannerDotnet.Core").SingleOrDefault();

            Assert.NotNull(coreProject);
            Assert.Equal(new FileInfo(Path.Combine(directoryInfo.FullName, @"DependencyScannerDotnet.Core\DependencyScannerDotnet.Core.csproj")).FullName, coreProject.FullFileName);
            Assert.True(coreProject.IsNewSdkStyle);
            Assert.NotNull(coreProject.Targets);
            Assert.Collection(
                coreProject.Targets,
                target => Assert.Equal("net5.0", target)
            );

            ProjectFile appProject = projects.Where(project => project.ProjectName == "DependencyScannerDotnet.App").SingleOrDefault();

            Assert.NotNull(appProject);
            Assert.Equal(new FileInfo(Path.Combine(directoryInfo.FullName, @"DependencyScannerDotnet.App\DependencyScannerDotnet.App.csproj")).FullName, appProject.FullFileName);
            Assert.True(appProject.IsNewSdkStyle);
            Assert.NotNull(appProject.Targets);
            Assert.Collection(
                appProject.Targets,
                target => Assert.Equal("net5.0-windows", target)
            );
            Assert.NotNull(appProject.ReferencedProjects);
            Assert.Collection(
                appProject.ReferencedProjects,
                project => Assert.True(project.ProjectName == "DependencyScannerDotnet.Core"
                                            && project.FullFileName == new DirectoryInfo(Path.Combine(directoryInfo.FullName, @"DependencyScannerDotnet.Core\DependencyScannerDotnet.Core.csproj")).FullName)
            );
            Assert.NotNull(appProject.ReferencedPackages);
            Assert.Equal(3, appProject.ReferencedPackages.Count);
            Assert.Contains(appProject.ReferencedPackages, package => package.Id == "NuniToolbox.Ui" && package.Version.ToString() == "1.0.0");

            ProjectFile testProject = projects.Where(project => project.ProjectName == "DependencyScannerDotnet.Core.Test").SingleOrDefault();

            Assert.NotNull(testProject);
            Assert.Equal(new FileInfo(Path.Combine(directoryInfo.FullName, @"DependencyScannerDotnet.Core.Test\DependencyScannerDotnet.Core.Test.csproj")).FullName, testProject.FullFileName);
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
                                            && project.FullFileName == new DirectoryInfo(Path.Combine(directoryInfo.FullName, @"DependencyScannerDotnet.Core\DependencyScannerDotnet.Core.csproj")).FullName)
            );
            Assert.NotNull(testProject.ReferencedPackages);
            Assert.Equal(4, testProject.ReferencedPackages.Count);
            Assert.Contains(testProject.ReferencedPackages, package => package.Id == "xunit" && package.Version.ToString() == "2.4.1");
        }
    }
}
