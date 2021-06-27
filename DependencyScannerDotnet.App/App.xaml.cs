using DependencyScannerDotnet.App.GuiLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace DependencyScannerDotnet.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs args)
        {
            base.OnStartup(args);

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            WindowViewModel windowViewModel = new();
            SelectProjectDirectoryViewModel viewModel = new(windowViewModel);
            windowViewModel.CurrentViewModel = viewModel;
            MainWindow mainWindow = new(windowViewModel);

            mainWindow.Show();
        }
    }
}
