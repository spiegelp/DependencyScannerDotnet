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

        public ConflictPackageViewModel(ConflictPackage conflictPackage)
            : base()
        {
            ConflictPackage = conflictPackage;
        }
    }
}
