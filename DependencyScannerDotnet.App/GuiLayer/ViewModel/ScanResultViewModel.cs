using DependencyScannerDotnet.Core.Model;
using DependencyScannerDotnet.Core.Services;
using MaterialDesignExtensions.Controls;
using NuniToolbox.Collections;
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

        public ICommand OpenConflictPackageCommand { get; init; }

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

        public ICommand SearchPackagesCommand { get; init; }

        public ExtendedObservableCollection<PackageWithReferencingProjects> SearchPackagesResult { get; init; }

        public ICommand SelectPackageForUpgradeCommand { get; init; }

        public ICommand UpdatePackageVersionCommand { get; init; }

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
            OpenConflictPackageCommand = new DelegateCommand<ConflictPackage>(OpenConflictPackageHandler);
            SearchPackagesCommand = new DelegateCommand<string>(SearchPackagesHandler);
            SelectPackageForUpgradeCommand = new DelegateCommand(SelectPackageForUpgradeHandler);
            UpdatePackageVersionCommand = new DelegateCommand<string>(UpdatePackageVersionHandler);

            SearchPackagesResult = new();
        }

        public async Task InitDirectoryAsync(string directory, ScanOptions scanOptions)
        {
            await ScanDirectoryAsync(directory, scanOptions).ConfigureAwait(false);
        }

        public async Task InitImportAsync(string file, ScanOptions scanOptions)
        {
            ImportExportService importExportService = new();
            ScanResult = await importExportService.ImportScanResultFromFileAsync(file).ConfigureAwait(false);

            DependencyScanner dependencyScanner = new(null, null, null);
            dependencyScanner.FindPackageVersionConflicts(ScanResult, scanOptions);

            PackageUpgrader packageUpgrader = new();
            ScanResult.PackageIdsForUpgrade = packageUpgrader.GetPackageIdsForUpgrade(ScanResult.Projects);
        }

        public async Task InitSolutionAsync(string file, ScanOptions scanOptions)
        {
            await ScanSolutionAsync(file, scanOptions).ConfigureAwait(false);
        }

        private async Task ScanDirectoryAsync(string directory, ScanOptions scanOptions)
        {
            DependencyScanner dependencyScanner = new(new DirectoryProjectSource(directory), new TargetFrameworkMappingService(), new PackageUpgrader());
            ScanResult = await dependencyScanner.ScanDependenciesAsync(scanOptions).ConfigureAwait(false);
        }

        private async Task ScanSolutionAsync(string solution, ScanOptions scanOptions)
        {
            DependencyScanner dependencyScanner = new(new SolutionProjectSource(solution), new TargetFrameworkMappingService(), new PackageUpgrader());
            ScanResult = await dependencyScanner.ScanDependenciesAsync(scanOptions).ConfigureAwait(false);
        }

        private void BackHandler()
        {
            WindowViewModel.CurrentViewModel = new SelectProjectSourceViewModel(WindowViewModel);
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

        public void OpenConflictPackageHandler(ConflictPackage conflictPackage)
        {
            if (conflictPackage != null)
            {
                WindowViewModel.OpenInRightDrawer(new ConflictPackageViewModel(this, conflictPackage));
            }
        }

        public async void SearchPackagesHandler(string searchTerm)
        {
            try
            {
                IsBusy = true;

                List<PackageWithReferencingProjects> result = await Task.Run(() =>
                {
                    DependencyScanner dependencyScanner = new(null, null, null);

                    return dependencyScanner.SearchPackagesInProjects(ScanResult, searchTerm);
                });

                RunWithinUiThread(() => SearchPackagesResult.ReplaceWith(result));
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void SelectPackageForUpgradeHandler()
        {
            if (ScanResult.PackageIdsForUpgrade != null && ScanResult.PackageIdsForUpgrade.Any())
            {
                WindowViewModel.OpenInRightDrawer(new SelectPackageForUpgradeViewModel(this));
            }
        }

        public async void UpdatePackageVersionHandler(string packageId)
        {
            PackageUpgradeViewModel nextViewModel = null;

            WindowViewModel.CloseDrawer();

            try
            {
                IsBusy = true;

                nextViewModel = await Task.Run(async () =>
                {
                    PackageUpgradeViewModel viewModel = new(WindowViewModel);
                    await viewModel.InitAsync(ScanResult, packageId);

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
