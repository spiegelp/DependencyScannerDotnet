using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Model
{
    public interface IDependency
    {
        string Name { get; }

        string Version { get; }

        List<IDependency> AllReferences { get; }
    }
}
