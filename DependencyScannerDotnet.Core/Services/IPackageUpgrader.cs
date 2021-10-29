using DependencyScannerDotnet.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public interface IPackageUpgrader
    {
        List<ProjectReference> GetProjectsForPackageUpdate(List<ProjectReference> projects, PackageReference package);

        Task UpdatePackageVersionAsync(List<ProjectReference> projects, PackageReference package, string newVersion);

        string UpdatePackageVersionNewSdkStyleProject(string xmlStr, string packageId, string newVersion);

        string UpdatePackageVersionLegacyProject(string xmlStr, string packageId, string newVersion);
    }
}
