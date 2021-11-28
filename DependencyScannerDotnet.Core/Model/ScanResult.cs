using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Model
{
    public class ScanResult
    {
        public List<ProjectReference> Projects { get; init; }

        public List<ConflictPackage> ConflictPackages { get; set; }

        public List<string> PackageIdsForUpgrade { get; set; }

        public ScanResult(List<ProjectReference> projects, List<ConflictPackage> conflictPackages, List<string> packageIdsForUpgrade)
        {
            Projects = projects;
            ConflictPackages = conflictPackages;
            PackageIdsForUpgrade = packageIdsForUpgrade;
        }
    }
}
