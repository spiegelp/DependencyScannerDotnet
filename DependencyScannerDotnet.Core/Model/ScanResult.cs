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

        public ScanResult(List<ProjectReference> projects)
        {
            Projects = projects;
        }
    }
}
