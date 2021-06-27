using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.App.GuiLayer.ViewModel
{
    public class WindowViewModel : NuniToolbox.Ui.ViewModel.WindowViewModel
    {
        public const string MainWindowDialogHostName = "mainWindowDialogHost";

        public WindowViewModel(ViewModelObject viewModel = null, bool disposeOldViewModel = true)
            : base(viewModel, disposeOldViewModel)
        {
        }
    }
}
