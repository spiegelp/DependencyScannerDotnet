using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Model
{
    public class ConflictPackage
    {
        public string PackageId { get; init; }

        public List<string> Versions { get; init; }

        public ConflictPackage(string packageId, List<string> versions)
        {
            PackageId = packageId;
            Versions = versions;
        }
    }
}
