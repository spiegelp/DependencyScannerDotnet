using DependencyScannerDotnet.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.App.GuiLayer.ViewModel
{
    public class ScanOptionsViewModel : NuniToolbox.Ui.ViewModel.ViewModelObject
    {
        public ScanOptions ScanOptions { get; init; }

        public ScanOptionsViewModel(ScanOptions scanOptions)
            : base()
        {
            ScanOptions = scanOptions;
        }
    }
}
