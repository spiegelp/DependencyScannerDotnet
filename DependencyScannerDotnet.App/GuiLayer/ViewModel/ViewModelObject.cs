using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.App.GuiLayer.ViewModel
{
    public abstract class ViewModelObject : NuniToolbox.Ui.ViewModel.ViewModelObject
    {
        public WindowViewModel WindowViewModel { get; private set; }

        public ViewModelObject(WindowViewModel windowViewModel)
            : base()
        {
            WindowViewModel = windowViewModel;
        }
    }
}
