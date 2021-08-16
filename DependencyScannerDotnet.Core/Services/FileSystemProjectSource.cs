using DependencyScannerDotnet.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public abstract class FileSystemProjectSource : ProjectSource
    {
        public FileSystemProjectSource() : base() { }

        protected async Task<List<ProjectFile>> ParseProjectFilesAsync(List<FileInfo> projectFileInfoList)
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

        protected async Task<ProjectFile> ParseProjectFileAsync(FileInfo projectFileInfo)
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(projectFileInfo.FullName).ConfigureAwait(false);

            ProjectFile project = ParseProjectFile(fileBytes);
            project.ProjectName = Path.GetFileNameWithoutExtension(projectFileInfo.Name);
            project.FullFileName = projectFileInfo.FullName;

            if (project.ReferencedProjects != null)
            {
                project.ReferencedProjects.ForEach(referencedProject =>
                {
                    referencedProject.FullFileName = new FileInfo(Path.Combine(projectFileInfo.DirectoryName, referencedProject.FullFileName)).FullName;
                });
            }

            return project;
        }
    }
}
