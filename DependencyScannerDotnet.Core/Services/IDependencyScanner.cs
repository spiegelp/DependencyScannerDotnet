using DependencyScannerDotnet.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public interface IDependencyScanner
    {
        Task<ScanResult> ScanDependenciesAsync(int maxDepth = 64);

        void FindPackageVersionConflicts(ScanResult scanResult);
    }
}
