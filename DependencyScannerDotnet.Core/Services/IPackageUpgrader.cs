using DependencyScannerDotnet.Core.Model;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public interface IPackageUpgrader
    {
        List<ProjectReference> GetProjectsForPackageUpdate(List<ProjectReference> projects, string packageId);

        Task<List<NuGetVersion>> GetVersionsForPackageAsync(string packageId, bool includePrerelease);

        Task UpdatePackageVersionAsync(List<ProjectReference> projects, string packageId, NuGetVersion newVersion);

        string UpdatePackageVersionNewSdkStyleProject(string xmlStr, string packageId, NuGetVersion newVersion);

        string UpdatePackageVersionLegacyProject(string xmlStr, string packageId, NuGetVersion newVersion);
    }
}
