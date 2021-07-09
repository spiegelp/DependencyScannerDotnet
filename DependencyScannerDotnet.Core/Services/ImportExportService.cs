using DependencyScannerDotnet.Core.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public class ImportExportService : IImportExportService
    {
        public ImportExportService() { }

        public async Task ExportScanResultAsync(ScanResult scanResult, string fileName)
        {
            ScanResultExport scanResultExport = PrepareScanResultExport(scanResult);
            string json = JsonConvert.SerializeObject(scanResultExport, CreateSettings());

            await File.WriteAllBytesAsync(fileName, Encoding.UTF8.GetBytes(json)).ConfigureAwait(false);
        }

        public async Task<ScanResult> ImportScanResultAsync(string fileName)
        {
            byte[] bytes = await File.ReadAllBytesAsync(fileName).ConfigureAwait(false);

            ScanResultExport scanResultExport = JsonConvert.DeserializeObject<ScanResultExport>(Encoding.UTF8.GetString(bytes), CreateSettings());

            return ReconstructScanResult(scanResultExport);
        }

        private ScanResultExport PrepareScanResultExport(ScanResult scanResult)
        {
            ScanResultExport scanResultExport = new();

            List<ProjectReference> projects = FlattenProjects(scanResult);
            List<PackageReference> packages = FlattenPackages(scanResult);

            Dictionary<ProjectReference, Guid> exportIdsByProject = new();
            Dictionary<Guid, ProjectReference> projectsByExportId = new();

            Dictionary<PackageReference, Guid> exportIdsByPackage = new();
            Dictionary<Guid, PackageReference> packagesByExportId = new();

            projects.ForEach(project =>
            {
                Guid id = Guid.NewGuid();
                ScanResultExport.Project projectExport = new() { Id = id, ProjectName = project.ProjectName, Version = project.Version, IsNewSdkStyle = project.IsNewSdkStyle, Targets = project.Targets.ToList() };

                exportIdsByProject[project] = id;
                projectsByExportId[id] = project;
                scanResultExport.Projects.Add(projectExport);

            });

            packages.ForEach(package =>
            {
                Guid id = Guid.NewGuid();
                ScanResultExport.Package packageExport = new() { Id = id, PackageId = package.PackageId, Version = package.Version };

                exportIdsByPackage[package] = id;
                packagesByExportId[id] = package;
                scanResultExport.Packages.Add(packageExport);
            });

            scanResultExport.Projects.ForEach(projectExport =>
            {
                ProjectReference project = projectsByExportId[projectExport.Id];

                if (project.ProjectReferences != null)
                {
                    project.ProjectReferences.ForEach(childProject =>
                    {
                        projectExport.ReferencedProjectIds.Add(exportIdsByProject[childProject]);
                    });
                }

                if (project.PackageReferences != null)
                {
                    project.PackageReferences.ForEach(childPackage =>
                    {
                        projectExport.ReferencedPackageIds.Add(exportIdsByPackage[childPackage]);
                    });
                }
            });

            scanResultExport.Packages.ForEach(packageExport =>
            {
                PackageReference package = packagesByExportId[packageExport.Id];

                if (package.PackageReferences != null)
                {
                    package.PackageReferences.ForEach(childPackage =>
                    {
                        packageExport.ReferencedPackageIds.Add(exportIdsByPackage[childPackage]);
                    });
                }
            });

            return scanResultExport;
        }

        private List<ProjectReference> FlattenProjects(ScanResult scanResult)
        {
            HashSet<ProjectReference> projects = new();

            scanResult.Projects.ForEach(project =>
            {
                projects.Add(project);

                if (project.ProjectReferences != null)
                {
                    project.ProjectReferences.ForEach(childProject => projects.Add(childProject));
                }
            });

            return projects.ToList();
        }

        private List<PackageReference> FlattenPackages(ScanResult scanResult)
        {
            HashSet<PackageReference> packages = new();

            scanResult.Projects.ForEach(project =>
            {
                project.PackageReferences.ForEach(package => FlattenPackages(package, packages));
            });

            return packages.ToList();
        }

        private void FlattenPackages(PackageReference package, HashSet<PackageReference> packages)
        {
            packages.Add(package);

            if (package.PackageReferences != null)
            {
                package.PackageReferences.ForEach(childPackage => FlattenPackages(childPackage, packages));
            }
        }

        private ScanResult ReconstructScanResult(ScanResultExport scanResultExport)
        {
            Dictionary<Guid, ProjectReference> projectsByExportId = new();
            Dictionary<Guid, PackageReference> packagesByExportId = new();

            scanResultExport.Projects.ForEach(projectExport =>
            {
                ProjectReference project = new() { ProjectName = projectExport.ProjectName, Version = projectExport.Version, IsNewSdkStyle = projectExport.IsNewSdkStyle, Targets = projectExport.Targets?.ToList() };

                projectsByExportId[projectExport.Id] = project;
            });

            scanResultExport.Packages.ForEach(packageExport =>
            {
                PackageReference package = new() { PackageId = packageExport.PackageId, Version = packageExport.Version };

                packagesByExportId[packageExport.Id] = package;
            });

            scanResultExport.Projects.ForEach(projectExport =>
            {
                ProjectReference project = projectsByExportId[projectExport.Id];

                if (projectExport.ReferencedProjectIds != null)
                {
                    projectExport.ReferencedProjectIds.ForEach(id => project.ProjectReferences.Add(projectsByExportId[id]));
                }

                if (projectExport.ReferencedPackageIds != null)
                {
                    projectExport.ReferencedPackageIds.ForEach(id => project.PackageReferences.Add(packagesByExportId[id]));
                }
            });

            scanResultExport.Packages.ForEach(packageExport =>
            {
                PackageReference package = packagesByExportId[packageExport.Id];

                if (packageExport.ReferencedPackageIds != null)
                {
                    packageExport.ReferencedPackageIds.ForEach(id => package.PackageReferences.Add(packagesByExportId[id]));
                }
            });

            return new(projectsByExportId.Values.ToList(), null);
        }

        private JsonSerializerSettings CreateSettings()
        {
            return new()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }
    }
}
