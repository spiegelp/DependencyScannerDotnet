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
    public abstract class ProjectSource : IProjectSource
    {

        public ProjectSource() { }

        public abstract Task<List<ProjectReference>> LoadProjectFilesAsync();

        protected ProjectReference ParseProjectFile(byte[] fileBytes)
        {
            ProjectReference projectReference = new();

            string fileStr = Encoding.UTF8.GetString(fileBytes);

            XmlDocument projectXml = new();
            projectXml.LoadXml(fileStr);
            XmlElement root = projectXml.DocumentElement;

            if (root.Name == "Project" && root.HasAttribute("Sdk"))
            {
                projectReference.IsNewSdkStyle = true;

                ParseNewSdkStyleProject(root, projectReference);
            }
            else
            {
                projectReference.IsNewSdkStyle = false;

                ParseLegacyProject(root, projectReference);
            }

            return projectReference;
        }

        private void ParseNewSdkStyleProject(XmlElement root, ProjectReference project)
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

                ProjectReference projectReference = new()
                {
                    ProjectName = referencedProjectName
                };

                project.ProjectReferences.Add(projectReference);
            }

            XmlNodeList packageReferenceNodeList = root.SelectNodes("ItemGroup/PackageReference");

            foreach (XmlElement packageReferenceNode in packageReferenceNodeList)
            {
                PackageReference packageReference = new()
                {
                    PackageId = packageReferenceNode.GetAttribute("Include"),
                    Version = packageReferenceNode.GetAttribute("Version")
                };

                project.PackageReferences.Add(packageReference);
            }


        }

        private void ParseLegacyProject(XmlElement root, ProjectReference projectReference)
        {
        }

        protected void ReplaceReferencedProjects(List<ProjectReference> projects)
        {
            Dictionary<string, ProjectReference> projectsByName = projects.ToDictionary(projectReference => projectReference.ProjectName);

            projects.ForEach(project =>
            {
                if (project.ProjectReferences != null)
                {
                    project.ProjectReferences.ToList().ForEach(projectReference =>
                    {
                        if (projectsByName.TryGetValue(projectReference.ProjectName, out ProjectReference item)) {
                            project.ProjectReferences.Remove(projectReference);
                            project.ProjectReferences.Add(item);
                        }
                    });
                }
            });
        }
    }
}
