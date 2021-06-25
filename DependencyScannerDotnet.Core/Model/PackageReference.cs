using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Model
{
    public class PackageReference
    {
        public string PackageId { get; set; }

        public string Version { get; set; }

        public List<PackageReference> PackageReferences { get; set; } = new();

        public PackageReference() { }
    }
}
