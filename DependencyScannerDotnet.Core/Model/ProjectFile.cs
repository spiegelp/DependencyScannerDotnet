using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Model
{
    public class ProjectFile
    {
        public string ProjectName { get; set; }

        public string Version { get; set; }

        public bool IsNewSdkStyle { get; set; }

        public string FullFileName { get; set; }

        public string UniqueKey
        {
            get
            {
                return $"{FullFileName}|{ProjectName}";
            }
        }

        public List<string> Targets { get; init; } = new();

        public List<ProjectFile> ReferencedProjects { get; set; } = new();

        public List<PackageIdentity> ReferencedPackages { get; set; } = new();

        public ProjectFile() { }
    }
}
