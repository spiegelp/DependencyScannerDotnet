using DependencyScannerDotnet.Core.Model;
using DependencyScannerDotnet.Core.Services;
using NuGet.Versioning;
using NuniToolbox.Collections;
using NuniToolbox.Ui.Commands;
using NuniToolbox.Ui.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DependencyScannerDotnet.App.GuiLayer.ViewModel
{
    public class PackageUpgradeViewModel : ViewModelObject
    {
        private IPackageUpgrader m_packageUpgrader;

        private ScanResult m_scanResult;

        private bool m_includePrerelease;
        private NuGetVersion m_selectedVersion;

        public ICommand BackCommand { get; init; }

        public bool HasSelectedVersion
        {
            get
            {
                return SelectedVersion != null;
            }
        }

        public bool IncludePrerelease
        {
            get
            {
                return m_includePrerelease;
            }

            set
            {
                if (m_includePrerelease != value)
                {
                    m_includePrerelease = value;

                    OnPropertyChanged(nameof(IncludePrerelease));

#pragma warning disable CS4014
                    UpdateVersionsAsync();
#pragma warning restore CS4014
                }
            }
        }

        public string PackageId { get; private set; }

        public List<SelectableViewModel<ProjectForPackageUpdate>> Projects { get; private set; }

        public NuGetVersion SelectedVersion
        {
            get
            {
                return m_selectedVersion;
            }

            set
            {
                m_selectedVersion = value;

                OnPropertyChanged(nameof(SelectedVersion));
                OnPropertyChanged(nameof(HasSelectedVersion));
            }
        }

        public ICommand UpdatePackageVersionCommand { get; init; }

        public ExtendedObservableCollection<NuGetVersion> Versions { get; init; }

        public PackageUpgradeViewModel(WindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            BackCommand = new DelegateCommand(BackHandler);
            UpdatePackageVersionCommand = new DelegateCommand(UpdatePackageVersionHandler);

            m_packageUpgrader = new PackageUpgrader();

            m_scanResult = null;

            m_includePrerelease = false;
            m_selectedVersion = null;

            Versions = new();
        }

        public async Task InitAsync(ScanResult scanResult, string packageId)
        {
            m_scanResult = scanResult;
            PackageId = packageId;

            Projects = m_packageUpgrader
                .GetProjectsForPackageUpdate(scanResult.Projects, packageId)
                .Select(project => new SelectableViewModel<ProjectForPackageUpdate>(project, false))
                .ToList();

            await UpdateVersionsAsync().ConfigureAwait(false);
        }

        private async Task UpdateVersionsAsync()
        {
            List<NuGetVersion> versions = await m_packageUpgrader.GetVersionsForPackageAsync(PackageId, IncludePrerelease).ConfigureAwait(false);

            Versions.ReplaceWith(versions);

            if (versions.Any())
            {
                if (SelectedVersion == null || !versions.Any(version => version.ToString() == SelectedVersion.ToString()))
                {
                    SelectedVersion = versions[0];
                }
            }
            else
            {
                SelectedVersion = null;
            }
        }

        private void BackHandler()
        {
            WindowViewModel.CurrentViewModel = new ScanResultViewModel(WindowViewModel) { ScanResult = m_scanResult };
        }

        private async void UpdatePackageVersionHandler()
        {
            try
            {
                IsBusy = true;

                await Task.Run(async () =>
                {
                    List<ProjectReference> projects = Projects
                        .Where(item => item.IsSelected)
                        .Select(item => item.Item.Project)
                        .ToList();

                    await m_packageUpgrader.UpdatePackageVersionAsync(projects, PackageId, SelectedVersion).ConfigureAwait(false);
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
