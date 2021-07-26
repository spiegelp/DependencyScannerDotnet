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

        private bool m_isRightDrawerOpen;
        private object m_rightDrawerContent;

        public bool IsRightDrawerOpen
        {
            get
            {
                return m_isRightDrawerOpen;
            }

            set
            {
                m_isRightDrawerOpen = value;

                OnPropertyChanged();
            }
        }

        public object RightDrawerContent
        {
            get
            {
                return m_rightDrawerContent;
            }

            set
            {
                m_rightDrawerContent = value;

                OnPropertyChanged();
            }
        }

        public WindowViewModel(ViewModelObject viewModel = null, bool disposeOldViewModel = true)
            : base(viewModel, disposeOldViewModel)
        {
            m_isRightDrawerOpen = false;
            m_rightDrawerContent = null;
        }

        public void OpenInRightDrawer(object content)
        {
            RightDrawerContent = content;
            IsRightDrawerOpen = true;
        }

        public void DrawerClosingHandler(object sender, MaterialDesignThemes.Wpf.DrawerClosingEventArgs args)
        {
            RightDrawerContent = null;
        }
    }
}
