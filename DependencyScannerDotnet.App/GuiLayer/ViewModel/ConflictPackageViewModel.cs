using DependencyScannerDotnet.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.App.GuiLayer.ViewModel
{
    public class ConflictPackageViewModel : NuniToolbox.Ui.ViewModel.ViewModelObject
    {
        public ConflictPackage ConflictPackage { get; init; }

        public ScanResultViewModel ScanResultViewModel { get; init; }

        public bool Upgradeable
        {
            get
            {
                return ScanResultViewModel.ScanResult.PackageIdsForUpgrade != null
                    && ScanResultViewModel.ScanResult.PackageIdsForUpgrade.Any(packageId => packageId == ConflictPackage.PackageId);
            }
        }

        public ConflictPackageViewModel(ScanResultViewModel scanResultViewModel, ConflictPackage conflictPackage)
            : base()
        {
            ScanResultViewModel = scanResultViewModel;
            ConflictPackage = conflictPackage;
        }
    }
}
