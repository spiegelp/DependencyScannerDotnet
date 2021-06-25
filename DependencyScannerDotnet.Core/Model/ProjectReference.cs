using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Model
{
    public class ProjectReference
    {
        public string ProjectName { get; set; }

        public string Version { get; set; }

        public bool IsNewSdkStyle { get; set; }

        public List<string> Targets { get; init; } = new();

        public List<ProjectReference> ProjectReferences { get; set; } = new();

        public List<PackageReference> PackageReferences { get; set; } = new();

        public ProjectReference() { }
    }
}
