using DependencyScannerDotnet.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DependencyScannerDotnet.Core.Test.Services
{
    public class TargetFrameworkMappingServiceTest
    {
        public TargetFrameworkMappingServiceTest() { }

        [Theory]
        [InlineData("net472", "net472")]
        [InlineData("netstandard2.1", "netstandard2.1")]
        [InlineData("netcoreapp3.1", "netcoreapp3.1")]
        [InlineData("net5.0", "netcore50")]
        [InlineData("net5.0-windows", "netcore50")]
        [InlineData("net6.0", "netcore60")]
        public void GetNugetFolderForTargetFramework_Ok(string targetFramework, string expected)
        {
            Assert.Equal(expected, new TargetFrameworkMappingService().GetNugetFolderForTargetFramework(targetFramework));
        }
    }
}
