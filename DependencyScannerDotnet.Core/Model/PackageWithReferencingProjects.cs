using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Model
{
    public class PackageWithReferencingProjects
    {
        public PackageReference Package { get; init; }

        public List<ProjectReference> Projects { get; init; }

        public PackageWithReferencingProjects(PackageReference package, List<ProjectReference> projects)
        {
            Package = package;
            Projects = projects;
        }
    }
}
