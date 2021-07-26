using DependencyScannerDotnet.App.GuiLayer.ViewModel;
using MaterialDesignExtensions.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DependencyScannerDotnet.App
{
    public partial class MainWindow : MaterialWindow
    {
        public WindowViewModel ViewModel { get; private set; }

        public MainWindow(WindowViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            m_drawerHost.DataContext = viewModel;
            m_contentControl.DataContext = viewModel;
            m_busyOverlay.DataContext = viewModel;

            m_drawerHost.DrawerClosing += viewModel.DrawerClosingHandler;
        }
    }
}
