using DependencyScannerDotnet.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public class SolutionProjectSource : FileSystemProjectSource
    {
        private readonly string m_solutionFile;

        public SolutionProjectSource(string solutionFile)
            : base()
        {
            m_solutionFile = solutionFile;
        }

        public override async Task<List<ProjectFile>> LoadProjectFilesAsync()
        {
            string solutionFileDirectory = new FileInfo(m_solutionFile).Directory.FullName;

            string[] solutionFileLines = await File.ReadAllLinesAsync(m_solutionFile, Encoding.UTF8).ConfigureAwait(false);

            List<FileInfo> projectFileInfoList = solutionFileLines
                .Where(line => line.StartsWith("Project("))
                .Select(line =>
                {
                    string[] parts = line[line.IndexOf("=")..].Split(",");

                    // project location should be at index 1
                    string projectFullFileName = Path.Combine(solutionFileDirectory, parts[1][(parts[1].IndexOf("\"") + 1)..parts[1].LastIndexOf("\"")]);

                    return new FileInfo(projectFullFileName);
                })
                .Where(fileInfo => fileInfo.Extension.ToLower() == ".csproj")
                .ToList();

            return await ParseProjectFilesAsync(projectFileInfoList).ConfigureAwait(false);
        }
    }
}
