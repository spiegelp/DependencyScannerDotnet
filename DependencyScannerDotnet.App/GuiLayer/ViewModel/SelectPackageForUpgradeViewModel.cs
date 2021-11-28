using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.App.GuiLayer.ViewModel
{
    public class SelectPackageForUpgradeViewModel : NuniToolbox.Ui.ViewModel.ViewModelObject
    {
        public ScanResultViewModel ScanResultViewModel { get; init; }

        public SelectPackageForUpgradeViewModel(ScanResultViewModel scanResultViewModel)
            : base()
        {
            ScanResultViewModel = scanResultViewModel;
        }
    }
}
