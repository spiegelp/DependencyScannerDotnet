using DependencyScannerDotnet.Core.Model;
using DependencyScannerDotnet.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.App.GuiLayer.ViewModel
{
    public class ScanResultViewModel : ViewModelObject
    {
        private ScanResult m_scanResult;

        public ScanResult ScanResult
        {
            get
            {
                return m_scanResult;
            }

            set
            {
                m_scanResult = value;

                OnPropertyChanged();
            }
        }

        public ScanResultViewModel(WindowViewModel windowViewModel) : base(windowViewModel) { }

        public async Task InitAsync(string directory)
        {
            await ScanAsync(directory).ConfigureAwait(false);
        }

        private async Task ScanAsync(string directory)
        {
            DependencyScanner dependencyScanner = new(new FileSystemProjectSource(directory), new TargetFrameworkMappingService());
            ScanResult = await dependencyScanner.ScanDependenciesAsync().ConfigureAwait(false);
        }
    }
}
