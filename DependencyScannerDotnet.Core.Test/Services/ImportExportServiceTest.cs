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
    public class ImportExportServiceTest
    {
        public ImportExportServiceTest() { }

        [Fact]
        public void ExportImport_Ok()
        {
            ProjectReference projectLib = new() { ProjectName = "Project.Lib", Version = "1.1.0", IsNewSdkStyle = true, FullFileName = @"C:\temp\Project.Lib\Project.Lib.csproj" };

            PackageReference packageA = new() { PackageId = "package.a", Version = "2.0.4" };
            projectLib.PackageReferences.Add(packageA);

            PackageReference packageB = new() { PackageId = "package.b", Version = "1.0.2" };
            packageA.PackageReferences.Add(packageB);

            ProjectReference projectApp = new() { ProjectName = "Project.App", Version = "1.0.1", IsNewSdkStyle = true, FullFileName = @"C:\temp\Project.App\Project.App.csproj" };
            projectApp.ProjectReferences.Add(projectLib);

            ScanResult scanResult = new(new List<ProjectReference> { projectApp, projectLib }, null, null);

            ImportExportService importExportService = new();
            string json = importExportService.ExportScanResultToJson(scanResult);

            Assert.NotNull(json);

            scanResult = importExportService.ImportScanResultFromJson(json);

            Assert.NotNull(scanResult);
            Assert.NotNull(scanResult.Projects);
            Assert.Collection(
                scanResult.Projects,
                x => Assert.True(x.ProjectName == projectApp.Name && x.Version == projectApp.Version && x.IsNewSdkStyle == projectApp.IsNewSdkStyle && x.FullFileName == projectApp.FullFileName),
                x => Assert.True(x.ProjectName == projectLib.Name && x.Version == projectLib.Version && x.IsNewSdkStyle == projectLib.IsNewSdkStyle && x.FullFileName == projectLib.FullFileName)
            );
            Assert.NotNull(scanResult.Projects[0].ProjectReferences);
            Assert.Collection(
                scanResult.Projects[0].ProjectReferences,
                x => Assert.True(x.ProjectName == projectLib.Name && x.Version == projectLib.Version && x.IsNewSdkStyle == projectLib.IsNewSdkStyle)
            );
            Assert.Equal(scanResult.Projects[1], scanResult.Projects[0].ProjectReferences[0]);
            Assert.NotNull(scanResult.Projects[1].PackageReferences);
            Assert.Collection(
                scanResult.Projects[1].PackageReferences,
                x => Assert.True(x.PackageId == packageA.PackageId && x.Version == packageA.Version)
            );
            Assert.NotNull(scanResult.Projects[1].PackageReferences);
            Assert.Collection(
                scanResult.Projects[1].PackageReferences[0].PackageReferences,
                x => Assert.True(x.PackageId == packageB.PackageId && x.Version == packageB.Version)
            );
        }
    }
}
