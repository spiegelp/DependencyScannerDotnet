using DependencyScannerDotnet.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public interface IProjectSource
    {
        Task<List<ProjectReference>> LoadProjectFilesAsync();
    }
}
