using DependencyScannerDotnet.Core.Model;
using DependencyScannerDotnet.Core.Services;
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
    public class ScanResultViewModel : ViewModelObject
    {
        private ScanResult m_scanResult;

        public ICommand BackCommand { get; init; }

        public ICommand ExportCommand { get; init; }

        public ScanResult ScanResult
        {
            get
            {
                return m_scanResult;
            }

            set
            {
                m_scanResult = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(VersionConflictsCount));
            }
        }

        public int VersionConflictsCount
        {
            get
            {
                return m_scanResult != null && m_scanResult.ConflictPackages != null ? m_scanResult.ConflictPackages.Count : 0;
            }
        }

        public ScanResultViewModel(WindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            BackCommand = new DelegateCommand(BackHandler);
            ExportCommand = new DelegateCommand(ExportHandler);
        }

        public async Task InitAsync(string directory, ScanOptions scanOptions)
        {
            await ScanAsync(directory, scanOptions).ConfigureAwait(false);
        }

        public async Task InitImportAsync(string file, ScanOptions scanOptions)
        {
            ImportExportService importExportService = new();
            ScanResult = await importExportService.ImportScanResultFromFileAsync(file).ConfigureAwait(false);

            DependencyScanner dependencyScanner = new(null, null);
            dependencyScanner.FindPackageVersionConflicts(ScanResult, scanOptions);
        }

        private async Task ScanAsync(string directory, ScanOptions scanOptions)
        {
            DependencyScanner dependencyScanner = new(new FileSystemProjectSource(directory), new TargetFrameworkMappingService());
            ScanResult = await dependencyScanner.ScanDependenciesAsync(scanOptions).ConfigureAwait(false);
        }

        private void BackHandler()
        {
            WindowViewModel.CurrentViewModel = new SelectProjectDirectoryViewModel(WindowViewModel);
        }

        private async void ExportHandler()
        {
            SaveFileDialogArguments saveFileDialogArgs = new()
            {
                Width = 600,
                Height = 800,
                CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Filters = "JSON|*.json",
                CreateNewDirectoryEnabled = true
            };

            SaveFileDialogResult result = await SaveFileDialog.ShowDialogAsync(WindowViewModel.MainWindowDialogHostName, saveFileDialogArgs);

            if (result.Confirmed)
            {
                string file = result.File;

                if (!file.ToLower().EndsWith(".json"))
                {
                    file = $"{file}.json";
                }

                bool writeFile = true;

                if (File.Exists(result.File))
                {
                    ConfirmationDialogArguments confirmationDialogArgs = new()
                    {
                        Message = LocalizationLayer.Strings.OverwriteExistingFile,
                        OkButtonLabel = LocalizationLayer.Strings.Yes.ToUpper(),
                        CancelButtonLabel = LocalizationLayer.Strings.No.ToUpper()
                    };

                    writeFile = await ConfirmationDialog.ShowDialogAsync(WindowViewModel.MainWindowDialogHostName, confirmationDialogArgs);
                }

                if (writeFile)
                {
                    try
                    {
                        IsBusy = true;

                        await Task.Run(async () =>
                        {
                            ImportExportService importExportService = new();
                            await importExportService.ExportScanResultToFileAsync(ScanResult, result.File).ConfigureAwait(false);
                        });
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }
            }
        }
    }
}
