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
    public class SelectProjectSourceViewModel : ViewModelObject
    {
        private readonly string m_defaultDirectory;

        public ICommand OpenScanOptionsCommand { get; init; }

        public ScanOptions ScanOptions { get; init; }

        public ICommand SelectDirectoryCommand { get; init; }

        public ICommand SelectImportFileCommand { get; init; }

        public ICommand SelectSolutionCommand { get; init; }

        public SelectProjectSourceViewModel(WindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            OpenScanOptionsCommand = new DelegateCommand(OpenScanOptionsHandler);
            SelectDirectoryCommand = new DelegateCommand(SelectDirectoryHandler);
            SelectImportFileCommand = new DelegateCommand(SelectImportFileHandler);
            SelectSolutionCommand = new DelegateCommand(SelectSolutionHandler);

            ScanOptions = new();

            m_defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

#if DEBUG
            string solutionDirectory = Path.Combine(new DirectoryInfo(new FileInfo(GetType().Assembly.Location).DirectoryName).FullName, @"..\..\..\..\");
            DirectoryInfo directory = new DirectoryInfo(solutionDirectory);

            if (directory.Exists)
            {
                m_defaultDirectory = directory.FullName;
            }
#endif
        }

        private void OpenScanOptionsHandler()
        {
            WindowViewModel.OpenInRightDrawer(new ScanOptionsViewModel(ScanOptions));
        }

        private async void SelectDirectoryHandler()
        {
            OpenDirectoryDialogArguments args = new()
            {
                Width = 600,
                Height = 800,
                CurrentDirectory = m_defaultDirectory
            };

            OpenDirectoryDialogResult result = await OpenDirectoryDialog.ShowDialogAsync(WindowViewModel.MainWindowDialogHostName, args);

            if (result.Confirmed)
            {
                ScanResultViewModel nextViewModel = null;

                try
                {
                    IsBusy = true;

                    nextViewModel = await Task.Run(async () =>
                    {
                        ScanResultViewModel viewModel = new(WindowViewModel);
                        await viewModel.InitDirectoryAsync(result.Directory, ScanOptions).ConfigureAwait(false);

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

        private async void SelectSolutionHandler()
        {
            OpenFileDialogArguments args = new()
            {
                Width = 600,
                Height = 800,
                CurrentDirectory = m_defaultDirectory,
                Filters = ".NET solution|*.sln"
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
                        await viewModel.InitSolutionAsync(result.File, ScanOptions).ConfigureAwait(false);

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
