using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace FIndFileChanged
{
    public class RecentlyOpenedFiles
    {
        private HashSet<string> recentPaths = new HashSet<string>();
        private HashSet<string> recentFilePaths = new HashSet<string>();

        public List<string> FindRecentlyOpenedFiles(string rootPath, TimeSpan timeSpan)
        {
            try
            {
                if (!Directory.Exists(rootPath))
                {
                    throw new DirectoryNotFoundException($"Directory does not exist: {rootPath}");
                }
                ScanDirectory(rootPath,timeSpan);
                return recentPaths.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error occurred: {ex.Message}");
                return new List<string>();
            }
        }

        private void ScanDirectory(string directoryPath,TimeSpan timeSpan)
        {
            DateTime previousTime = DateTime.Now - timeSpan;

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
                if (dirInfo.LastAccessTime >= previousTime)
                {
                    recentPaths.Add(directoryPath);

                    // Scanning Documents
                    foreach (string filePath in Directory.EnumerateFiles(directoryPath))
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        if (fileInfo.LastAccessTime >= previousTime)
                        {
                            recentFilePaths.Add(filePath);
                        }
                    }

                    // Recursive scanning of subfolders
                    foreach (string subDir in Directory.EnumerateDirectories(directoryPath))
                    {
                        ScanDirectory(subDir, timeSpan);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"No access to directories: {directoryPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Scanning the directoriy {directoryPath} ,error: {ex.Message}");
            }

        }
    }
    
}

// Note ：this is not a successful program!!!
//because Directory.EnumerateFiles(directoryPath),Directory.EnumerateFiles(directoryPath))
//It triggers the windows file LastAccessTime mechanism.
//This causes the LastAccessTime of all files to be updated to the current time,
//so that the judgment of fileInfo.LastAccessTime >= previousTime is meaningless,
//since all files will match the condition.

//