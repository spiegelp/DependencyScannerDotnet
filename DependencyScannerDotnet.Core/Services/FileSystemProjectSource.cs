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

        public override async Task<List<ProjectReference>> LoadProjectFilesAsync()
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

        private async Task<List<ProjectReference>> ParseProjectFilesAsync(List<FileInfo> projectFileInfoList)
        {
            List<ProjectReference> projectReferences = new();

            foreach (FileInfo projectFileInfo in projectFileInfoList)
            {
                ProjectReference projectReference = await ParseProjectFileAsync(projectFileInfo).ConfigureAwait(false);
                projectReferences.Add(projectReference);
            }

            ReplaceReferencedProjects(projectReferences);

            projectReferences.ForEach(projectReference =>
            {
                if (projectReference.ProjectReferences != null && projectReference.ProjectReferences.Any())
                {
                    projectReference.ProjectReferences = projectReference.ProjectReferences
                        .OrderBy(x => x.ProjectName.ToLower())
                        .ToList();
                }

                if (projectReference.PackageReferences != null && projectReference.PackageReferences.Any())
                {
                    projectReference.PackageReferences = projectReference.PackageReferences
                        .OrderBy(x => x.PackageId.ToLower())
                        .ToList();
                }
            });

            return projectReferences
                .OrderBy(projectReference => projectReference.ProjectName.ToLower())
                .ToList();
        }

        private async Task<ProjectReference> ParseProjectFileAsync(FileInfo projectFileInfo)
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(projectFileInfo.FullName).ConfigureAwait(false);

            ProjectReference projectReference = ParseProjectFile(fileBytes);
            projectReference.ProjectName = Path.GetFileNameWithoutExtension(projectFileInfo.Name);

            return projectReference;
        }
    }
}
