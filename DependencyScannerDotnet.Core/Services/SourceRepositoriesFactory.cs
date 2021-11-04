using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public static class SourceRepositoriesFactory
    {
        public static List<SourceRepository> GetSourceRepositories()
        {
            List<SourceRepository> sourceRepositories = new();

            // first look into the global package folder on the local disk
            string globalPackageFolder = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

            if (string.IsNullOrWhiteSpace(globalPackageFolder) || !Directory.Exists(globalPackageFolder))
            {
                globalPackageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
            }

            if (!string.IsNullOrWhiteSpace(globalPackageFolder) && Directory.Exists(globalPackageFolder))
            {
                sourceRepositories.Add(Repository.Factory.GetCoreV3(globalPackageFolder, FeedType.FileSystemV3));
            }

            // then use the feeds from the default config
            ISettings settings = Settings.LoadDefaultSettings(null);
            IEnumerable<PackageSource> packageSources = PackageSourceProvider.LoadPackageSources(settings);
            PackageSourceProvider packageSourceProvider = new(settings, packageSources);
            SourceRepositoryProvider sourceRepositoryProvider = new(packageSourceProvider, Repository.Provider.GetCoreV3());

            foreach (SourceRepository sourceRepository in sourceRepositoryProvider.GetRepositories())
            {
                sourceRepositories.Add(sourceRepository);
            }

            // the official feed, if it is not already there
            string officialFeedUri = "https://api.nuget.org/v3/index.json";

            if (!sourceRepositories.Any(sourceRepository => sourceRepository.PackageSource.Source != null && sourceRepository.PackageSource.Source.ToLower() == officialFeedUri))
            {
                sourceRepositories.Add(Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json", FeedType.HttpV3));
            }

            return sourceRepositories;
        }
    }
}
