using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public class TargetFrameworkMappingService : ITargetFrameworkMappingService
    {
        public TargetFrameworkMappingService() { }

        public string GetNugetFolderForTargetFramework(string targetFramework)
        {
            return targetFramework switch
            {
                "net5.0" => "netcore50",
                "net5.0-windows" => "netcore50",
                "net6.0" => "netcore60",
                _ => targetFramework
            };
        }
    }
}
