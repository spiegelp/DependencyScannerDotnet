using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Model
{
    public class PackageReference : IDependency
    {
        public string PackageId { get; set; }

        public string Version { get; set; }

        public List<PackageReference> PackageReferences { get; set; } = new();

        public string Name
        {
            get
            {
                return PackageId;
            }
        }

        public List<IDependency> AllReferences
        {
            get
            {
                return new(PackageReferences);
            }
        }

        public PackageReference() { }
    }
}
