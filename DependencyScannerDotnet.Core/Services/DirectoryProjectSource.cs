using DependencyScannerDotnet.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public class DirectoryProjectSource : FileSystemProjectSource
    {
        private readonly string m_baseDirectory;

        public DirectoryProjectSource(string baseDirectory)
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
    }
}
