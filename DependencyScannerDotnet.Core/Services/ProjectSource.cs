using DependencyScannerDotnet.Core.Model;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DependencyScannerDotnet.Core.Services
{
    public abstract class ProjectSource : IProjectSource
    {
        public ProjectSource() { }

        public abstract Task<List<ProjectFile>> LoadProjectFilesAsync();

        protected ProjectFile ParseProjectFile(byte[] fileBytes)
        {
            // get rid of the BOM (XML API does not like it)
            if (fileBytes.Length > 3 && fileBytes[0] == 239 && fileBytes[1] == 187 && fileBytes[2] == 191)
            {
                fileBytes = fileBytes[3..];
            }

            ProjectFile project = new();

            string fileStr = Encoding.UTF8.GetString(fileBytes);

            XmlDocument projectXml = new();
            projectXml.LoadXml(fileStr);
            XmlElement root = projectXml.DocumentElement;

            if (root.Name == "Project" && root.HasAttribute("Sdk"))
            {
                project.IsNewSdkStyle = true;

                ParseNewSdkStyleProject(root, project);
            }
            else
            {
                project.IsNewSdkStyle = false;

                ParseLegacyProject(root, project);
            }

            return project;
        }

        private void ParseNewSdkStyleProject(XmlElement root, ProjectFile project)
        {
            XmlNode node = root.SelectSingleNode("PropertyGroup/Version");

            if (node != null && !string.IsNullOrWhiteSpace(node.InnerText))
            {
                project.Version = node.InnerText;
            }

            XmlNode targetFrameworkNode = root.SelectSingleNode("PropertyGroup/TargetFramework");

            if (targetFrameworkNode != null && !string.IsNullOrWhiteSpace(targetFrameworkNode.InnerText))
            {
                project.Targets.Add(targetFrameworkNode.InnerText);
            }
            else
            {
                targetFrameworkNode = root.SelectSingleNode("PropertyGroup/TargetFrameworks");

                if (targetFrameworkNode != null && !string.IsNullOrWhiteSpace(targetFrameworkNode.InnerText))
                {
                    foreach (string target in targetFrameworkNode.InnerText.Split(';'))
                    {
                        project.Targets.Add(target.Trim());
                    }
                }
            }

            XmlNodeList projectReferenceNodeList = root.SelectNodes("ItemGroup/ProjectReference");

            foreach (XmlElement projectReferenceNode in projectReferenceNodeList)
            {
                string referencedProjectName = projectReferenceNode.GetAttribute("Include");
                referencedProjectName = referencedProjectName.Replace('\\', '/');
                referencedProjectName = Path.GetFileNameWithoutExtension(referencedProjectName);

                ProjectFile referencedProject = new()
                {
                    ProjectName = referencedProjectName
                };

                project.ReferencedProjects.Add(referencedProject);
            }

            XmlNodeList packageReferenceNodeList = root.SelectNodes("ItemGroup/PackageReference");

            foreach (XmlElement packageReferenceNode in packageReferenceNodeList)
            {
                PackageIdentity packageIdentity = new(packageReferenceNode.GetAttribute("Include"), new NuGetVersion(packageReferenceNode.GetAttribute("Version")));

                project.ReferencedPackages.Add(packageIdentity);
            }
        }

        private void ParseLegacyProject(XmlElement root, ProjectFile project)
        {
            XmlNode targetFrameworkNode = root.SelectSingleNode("PropertyGroup/TargetFrameworkVersion");

            if (targetFrameworkNode != null && !string.IsNullOrWhiteSpace(targetFrameworkNode.InnerText))
            {
                project.Targets.Add(targetFrameworkNode.InnerText[1..].Replace(".", string.Empty));
            }

            XmlNodeList projectReferenceNodeList = root.SelectNodes("ItemGroup/ProjectReference");

            foreach (XmlElement projectReferenceNode in projectReferenceNodeList)
            {
                string referencedProjectName = projectReferenceNode.GetAttribute("Include");
                referencedProjectName = referencedProjectName.Replace('\\', '/');
                referencedProjectName = Path.GetFileNameWithoutExtension(referencedProjectName);

                ProjectFile referencedProject = new()
                {
                    ProjectName = referencedProjectName
                };

                project.ReferencedProjects.Add(referencedProject);
            }

            XmlNodeList packageReferenceNodeList = root.SelectNodes("ItemGroup/PackageReference");

            foreach (XmlElement packageReferenceNode in packageReferenceNodeList)
            {
                PackageIdentity packageIdentity = new(packageReferenceNode.GetAttribute("Include"), new NuGetVersion(packageReferenceNode.GetAttribute("Version")));

                project.ReferencedPackages.Add(packageIdentity);
            }
        }

        protected void ReplaceReferencedProjects(List<ProjectFile> projects)
        {
            Dictionary<string, ProjectFile> projectsByName = projects.ToDictionary(project => project.ProjectName);
            Dictionary<PackageIdentity, PackageReference> packageReferenceCache = new(PackageIdentityComparer.Default);

            projects.ForEach(project =>
            {
                if (project.ReferencedProjects != null)
                {
                    project.ReferencedProjects.ToList().ForEach(referencedProject =>
                    {
                        if (projectsByName.TryGetValue(referencedProject.ProjectName, out ProjectFile item))
                        {
                            project.ReferencedProjects.Remove(referencedProject);
                            project.ReferencedProjects.Add(item);
                        }
                    });
                }
            });
        }
    }
}
