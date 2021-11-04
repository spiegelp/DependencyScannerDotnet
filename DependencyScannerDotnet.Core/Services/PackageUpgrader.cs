using DependencyScannerDotnet.Core.Model;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace DependencyScannerDotnet.Core.Services
{
    public class PackageUpgrader : IPackageUpgrader
    {
        public PackageUpgrader() { }

        public List<ProjectReference> GetProjectsForPackageUpdate(List<ProjectReference> projects, string packageId)
        {
            return projects
                .Where(project => !string.IsNullOrWhiteSpace(project.FullFileName)
                                        && File.Exists(project.FullFileName)
                                        && project.PackageReferences.Any(packageReference => packageReference.PackageId == packageId))
                .ToList();
        }

        public async Task<List<NuGetVersion>> GetVersionsForPackageAsync(string packageId, bool includePrerelease)
        {
            using SourceCacheContext cache = new();

            List<SourceRepository> sourceRepositories = SourceRepositoriesFactory.GetSourceRepositories();

            CancellationToken cancellationToken = CancellationToken.None;

            foreach (SourceRepository sourceRepository in sourceRepositories)
            {
                FeedType feedType = await sourceRepository.GetFeedType(cancellationToken).ConfigureAwait(false);

                if (feedType == FeedType.HttpV3)
                {
                    FindPackageByIdResource findPackageByIdResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>().ConfigureAwait(false);

                    IEnumerable<NuGetVersion> versions = await findPackageByIdResource.GetAllVersionsAsync(packageId, cache, NullLogger.Instance, cancellationToken).ConfigureAwait(false);

                    if (versions != null && versions.Any())
                    {
                        if (includePrerelease)
                        {
                            return versions.ToList();
                        }
                        else
                        {
                            return versions.Where(version => !version.IsPrerelease).ToList();
                        }
                    }
                }
            }

            return new();
        }

        public async Task UpdatePackageVersionAsync(List<ProjectReference> projects, string packageId, NuGetVersion newVersion)
        {
            projects = GetProjectsForPackageUpdate(projects, packageId);

            foreach (ProjectReference project in projects)
            {
                string projectXml = await ReadProjectXmlAsync(project).ConfigureAwait(false);

                string newProjectXml = project.IsNewSdkStyle
                    ? UpdatePackageVersionNewSdkStyleProject(projectXml, packageId, newVersion)
                    : UpdatePackageVersionLegacyProject(projectXml, packageId, newVersion);

                await WriteProjectXmlAsync(project.FullFileName, newProjectXml).ConfigureAwait(false);
            }
        }

        public string UpdatePackageVersionNewSdkStyleProject(string xmlStr, string packageId, NuGetVersion newVersion)
        {
            XmlDocument projectXml = new();
            projectXml.LoadXml(xmlStr);
            XmlElement root = projectXml.DocumentElement;

            XmlNodeList packageReferenceNodeList = root.SelectNodes($"ItemGroup/PackageReference[@Include='{packageId}']");

            foreach (XmlElement packageReferenceNode in packageReferenceNodeList)
            {
                packageReferenceNode.SetAttribute("Version", newVersion.ToString());
            }

            return WriteToXmlString(projectXml);
        }

        public string UpdatePackageVersionLegacyProject(string xmlStr, string packageId, NuGetVersion newVersion)
        {
            XmlDocument projectXml = new();
            projectXml.LoadXml(xmlStr);
            XmlElement root = projectXml.DocumentElement;

            XmlNamespaceManager namespaceManager = new(projectXml.NameTable);
            namespaceManager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");

            XmlNodeList packageReferenceNodeList = root.SelectNodes($"msbuild:ItemGroup/msbuild:PackageReference[@Include='{packageId}']", namespaceManager);

            foreach (XmlElement packageReferenceNode in packageReferenceNodeList)
            {
                XmlNode versionNode = packageReferenceNode.SelectSingleNode("msbuild:Version", namespaceManager);

                if (versionNode != null)
                {
                    versionNode.InnerText = newVersion.ToString();
                }
            }

            return WriteToXmlString(projectXml);
        }

        private async Task<string> ReadProjectXmlAsync(ProjectReference project)
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(project.FullFileName).ConfigureAwait(false);

            return Encoding.UTF8.GetString(fileBytes.GetRidOfBom());
        }

        private async Task WriteProjectXmlAsync(string fullFileName, string projectXml)
        {
            await File.WriteAllBytesAsync(fullFileName, Encoding.UTF8.GetBytes(projectXml)).ConfigureAwait(false);
        }

        private string WriteToXmlString(XmlDocument xmlDocument)
        {
            using MemoryStream ms = new();
            using XmlTextWriter xmlWriter = new(ms, Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                IndentChar = ' ',
                Indentation = 2
            };

            xmlDocument.WriteContentTo(xmlWriter);
            xmlWriter.Flush();

            byte[] formattedBytes = ms.ToArray();

            return Encoding.UTF8.GetString(formattedBytes.GetRidOfBom());
        }
    }
}
