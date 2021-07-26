using DependencyScannerDotnet.Core.Model;
using MaterialDesignExtensions.Controls;
using NuniToolbox.Ui.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DependencyScannerDotnet.App.GuiLayer.ViewModel
{
    public class SelectProjectDirectoryViewModel : ViewModelObject
    {
        private string m_selectedDirectory;

        public bool HasSelectedDirectory
        {
            get
            {
                return !string.IsNullOrWhiteSpace(m_selectedDirectory);
            }
        }

        public ICommand OpenScanOptionsCommand { get; init; }

        public ICommand ScanCommand { get; init; }

        public ScanOptions ScanOptions { get; init; }

        public string SelectedDirectory
        {
            get
            {
                return m_selectedDirectory;
            }

            set
            {
                m_selectedDirectory = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedDirectory));
            }
        }

        public ICommand SelectDirectoryCommand { get; init; }

        public ICommand SelectImportFileCommand { get; init; }

        public SelectProjectDirectoryViewModel(WindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            OpenScanOptionsCommand = new DelegateCommand(OpenScanOptionsHandler);
            ScanCommand = new DelegateCommand(ScanHandler);
            SelectDirectoryCommand = new DelegateCommand(SelectDirectoryHandler);
            SelectImportFileCommand = new DelegateCommand(SelectImportFileHandler);

            ScanOptions = new();

#if DEBUG
            string solutionDirectory = Path.Combine(new DirectoryInfo(new FileInfo(GetType().Assembly.Location).DirectoryName).FullName, @"..\..\..\..\");
            DirectoryInfo directory = new DirectoryInfo(solutionDirectory);

            if (directory.Exists)
            {
                SelectedDirectory = directory.FullName;
            }
#endif
        }

        private void OpenScanOptionsHandler()
        {
            WindowViewModel.OpenInRightDrawer(new ScanOptionsViewModel(ScanOptions));
        }

        private async void ScanHandler()
        {
            if (HasSelectedDirectory)
            {
                ScanResultViewModel nextViewModel = null;

                try
                {
                    IsBusy = true;

                    nextViewModel = await Task.Run(async () =>
                    {
                        ScanResultViewModel viewModel = new(WindowViewModel);
                        await viewModel.InitAsync(SelectedDirectory, ScanOptions).ConfigureAwait(false);

                        return viewModel;
                    });
                }
                finally
                {
                    IsBusy = false;
                }

                WindowViewModel.CurrentViewModel = nextViewModel;
            }
        }

        private async void SelectDirectoryHandler()
        {
            OpenDirectoryDialogArguments args = new()
            {
                Width = 600,
                Height = 800,
                CurrentDirectory = SelectedDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            OpenDirectoryDialogResult result = await OpenDirectoryDialog.ShowDialogAsync(WindowViewModel.MainWindowDialogHostName, args);

            if (result.Confirmed)
            {
                SelectedDirectory = result.Directory;
            }
        }

        private async void SelectImportFileHandler()
        {
            OpenFileDialogArguments args = new()
            {
                Width = 600,
                Height = 800,
                CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Filters = "JSON|*.json"
            };

            OpenFileDialogResult result = await OpenFileDialog.ShowDialogAsync(WindowViewModel.MainWindowDialogHostName, args);

            if (result.Confirmed)
            {
                ScanResultViewModel nextViewModel = null;

                try
                {
                    IsBusy = true;

                    nextViewModel = await Task.Run(async () =>
                    {
                        ScanResultViewModel viewModel = new(WindowViewModel);
                        await viewModel.InitImportAsync(result.File, ScanOptions).ConfigureAwait(false);

                        return viewModel;
                    });
                }
                finally
                {
                    IsBusy = false;
                }

                WindowViewModel.CurrentViewModel = nextViewModel;
            }
        }
    }
}
