using DependencyScannerDotnet.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DependencyScannerDotnet.Core.Services
{
    public class PackageUpgrader : IPackageUpgrader
    {
        public PackageUpgrader() { }

        public List<ProjectReference> GetProjectsForPackageUpdate(List<ProjectReference> projects, PackageReference package)
        {
            return projects
                .Where(project => !string.IsNullOrWhiteSpace(project.FullFileName)
                                        && File.Exists(project.FullFileName)
                                        && project.PackageReferences.Any(packageReference => packageReference.PackageId == package.PackageId))
                .ToList();
        }

        public async Task UpdatePackageVersionAsync(List<ProjectReference> projects, PackageReference package, string newVersion)
        {
            projects = GetProjectsForPackageUpdate(projects, package);

            foreach (ProjectReference project in projects)
            {
                string projectXml = await ReadProjectXmlAsync(project).ConfigureAwait(false);

                string newProjectXml = project.IsNewSdkStyle
                    ? UpdatePackageVersionNewSdkStyleProject(projectXml, package.PackageId, newVersion)
                    : UpdatePackageVersionLegacyProject(projectXml, package.PackageId, newVersion);

                await WriteProjectXmlAsync(project.FullFileName, newProjectXml).ConfigureAwait(false);
            }
        }

        public string UpdatePackageVersionNewSdkStyleProject(string xmlStr, string packageId, string newVersion)
        {
            XmlDocument projectXml = new();
            projectXml.LoadXml(xmlStr);
            XmlElement root = projectXml.DocumentElement;

            XmlNodeList packageReferenceNodeList = root.SelectNodes($"ItemGroup/PackageReference[@Include='{packageId}']");

            foreach (XmlElement packageReferenceNode in packageReferenceNodeList)
            {
                packageReferenceNode.SetAttribute("Version", newVersion);
            }

            return WriteToXmlString(projectXml);
        }

        public string UpdatePackageVersionLegacyProject(string xmlStr, string packageId, string newVersion)
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
                    versionNode.InnerText = newVersion;
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
