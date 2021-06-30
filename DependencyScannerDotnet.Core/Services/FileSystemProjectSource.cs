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
    public class FileSystemProjectSource : ProjectSource
    {
        private readonly string m_baseDirectory;

        public FileSystemProjectSource(string baseDirectory)
            : base()
        {
            m_baseDirectory = baseDirectory;
        }

        public override async Task<List<ProjectFile>> LoadProjectFilesAsync()
        {
            List<FileInfo> projectFileInfoList = new();

            FindFiles(m_baseDirectory, projectFileInfoList);

            return await ParseProjectFilesAsync(projectFileInfoList).ConfigureAwait(false);
        }

        private void FindFiles(string directory, List<FileInfo> projectFileInfoList)
        {
            DirectoryInfo directoryInfo = new(directory);

            directoryInfo.EnumerateFiles()
                .Where(fileInfo => fileInfo.Extension.ToLower() == ".csproj")
                .ToList()
                .ForEach(projectFileInfo => projectFileInfoList.Add(projectFileInfo));

            foreach (DirectoryInfo subDirectoryInfo in directoryInfo.EnumerateDirectories())
            {
                FindFiles(subDirectoryInfo.FullName, projectFileInfoList);
            }
        }

        private async Task<List<ProjectFile>> ParseProjectFilesAsync(List<FileInfo> projectFileInfoList)
        {
            List<ProjectFile> projects = new();

            foreach (FileInfo projectFileInfo in projectFileInfoList)
            {
                ProjectFile project = await ParseProjectFileAsync(projectFileInfo).ConfigureAwait(false);
                projects.Add(project);
            }

            ReplaceReferencedProjects(projects);

            projects.ForEach(projectReference =>
            {
                if (projectReference.ReferencedProjects != null && projectReference.ReferencedProjects.Any())
                {
                    projectReference.ReferencedProjects = projectReference.ReferencedProjects
                        .OrderBy(x => x.ProjectName.ToLower())
                        .ToList();
                }

                if (projectReference.ReferencedPackages != null && projectReference.ReferencedPackages.Any())
                {
                    projectReference.ReferencedPackages = projectReference.ReferencedPackages
                        .OrderBy(x => x.Id.ToLower())
                        .ToList();
                }
            });

            return projects
                .OrderBy(projectReference => projectReference.ProjectName.ToLower())
                .ToList();
        }

        private async Task<ProjectFile> ParseProjectFileAsync(FileInfo projectFileInfo)
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(projectFileInfo.FullName).ConfigureAwait(false);

            ProjectFile project = ParseProjectFile(fileBytes);
            project.ProjectName = Path.GetFileNameWithoutExtension(projectFileInfo.Name);

            return project;
        }
    }
}
