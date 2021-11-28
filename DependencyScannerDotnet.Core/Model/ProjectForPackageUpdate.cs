using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Model
{
    public class ProjectForPackageUpdate
    {
        public PackageReference CurrentPackage { get; init; }

        public ProjectReference Project { get; init; }

        public ProjectForPackageUpdate(ProjectReference project, PackageReference currentPackage)
        {
            Project = project;
            CurrentPackage = currentPackage;
        }
    }
}
