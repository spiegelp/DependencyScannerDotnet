using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public interface ITargetFrameworkMappingService
    {
        string GetNugetFolderForTargetFramework(string targetFramework);
    }
}
