using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Model
{
    public class ScanResultExport
    {
        public List<Project> Projects { get; init; } = new();

        public List<Package> Packages { get; init; } = new();

        public ScanResultExport() { }

        public class Project
        {
            public Guid Id { get; set; }

            public string ProjectName { get; set; }

            public string Version { get; set; }

            public bool IsNewSdkStyle { get; set; }

            public string FullFileName { get; set; }

            public List<string> Targets { get; init; } = new();

            public List<Guid> ReferencedProjectIds { get; set; } = new();

            public List<Guid> ReferencedPackageIds { get; set; } = new();

            public Project() { }
        }

        public class Package
        {
            public Guid Id { get; set; }

            public string PackageId { get; set; }

            public string Version { get; set; }

            public List<Guid> ReferencedPackageIds { get; set; } = new();

            public Package() { }
        }
    }
}
