using FIndFileChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIndFileChanged
{
     public class testLAT
    {
        //仅通过文件系统API很难准确追踪文件的实际访问历史，因为查询本身就会影响查询结果。
        //这个类用来测试函数对文件的操作是否触发了windows的 LastAccessTime机制。

        //in bin floder
        public void CreateTestFolders()
        {
            string baseDir = Path.Combine(Directory.GetCurrentDirectory(), "TestRoot");

            // Create Test1 and Test2 folders
            string[] testFolders = { "Test1", "Test2" };
            foreach (string folder in testFolders)
            {
                string folderPath = Path.Combine(baseDir, folder);
                Directory.CreateDirectory(folderPath);

                // Create three different types of files
                File.WriteAllText(Path.Combine(folderPath, "document.txt"), "This is a text file");
                File.WriteAllText(Path.Combine(folderPath, "config.json"), "{ \"setting\": \"value\" }");
                File.WriteAllBytes(Path.Combine(folderPath, "data.bin"), new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F });

                // Creating subfolders and their files
                string subFolderPath = Path.Combine(folderPath, "SubFolder");
                Directory.CreateDirectory(subFolderPath);
                File.WriteAllText(Path.Combine(subFolderPath, "inner.log"), "This is a log file");
            }
        }


        public  void GetAccessTimes()
        {
            string baseDir = Path.Combine(Directory.GetCurrentDirectory(), "TestRoot");
            string[] testFolders = { "Test1", "Test2" };

            foreach (string folder in testFolders)
            {
                string folderPath = Path.Combine(baseDir, folder);
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

                Console.WriteLine($"\n{folder} Folder Access Time: {dirInfo.LastAccessTime}");

                // Get file access time
                string[] files = {
                   Path.Combine(folderPath, "document.txt"),
                   Path.Combine(folderPath, "config.json"),
                   Path.Combine(folderPath, "data.bin")
                   };

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    Console.WriteLine($"{Path.GetFileName(file)} Access Time: {fileInfo.LastAccessTime}");
                }

                // Get access times for subfolders and their files
                string subFolderPath = Path.Combine(folderPath, "SubFolder");
                DirectoryInfo subDirInfo = new DirectoryInfo(subFolderPath);
                Console.WriteLine($"SubFolder Access Time: {subDirInfo.LastAccessTime}");

                FileInfo innerFile = new FileInfo(Path.Combine(subFolderPath, "inner.log"));
                Console.WriteLine($"inner.log Access Time: {innerFile.LastAccessTime}");
            }
        }


        public  void CheckAccessTimeChanges(Action testFunction)
        {
            string baseDir = Path.Combine(Directory.GetCurrentDirectory(), "TestRoot");
            string[] testFolders = { "Test1", "Test2" };

            // Store the initial time of files and directories separately
            Dictionary<string, DateTime> fileInitialTimes = new Dictionary<string, DateTime>();
            Dictionary<string, DateTime> dirInitialTimes = new Dictionary<string, DateTime>();

            // Record initial time
            void RecordInitialTimes()
            {
                foreach (string folder in testFolders)
                {
                    string folderPath = Path.Combine(baseDir, folder);
                    DirectoryInfo dirInfo = new DirectoryInfo(folderPath);

                    // Record folder time
                    dirInitialTimes[folderPath] = dirInfo.LastAccessTime;

                    // Recording of document time
                    string[] files = {
                        Path.Combine(folderPath, "document.txt"),
                        Path.Combine(folderPath, "config.json"),
                        Path.Combine(folderPath, "data.bin")
                        };

                    foreach (string file in files)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        fileInitialTimes[file] = fileInfo.LastAccessTime;
                    }

                    // Record subfolders and their file times
                    string subFolderPath = Path.Combine(folderPath, "SubFolder");
                    DirectoryInfo subDirInfo = new DirectoryInfo(subFolderPath);
                    dirInitialTimes[subFolderPath] = subDirInfo.LastAccessTime;

                    string innerFilePath = Path.Combine(subFolderPath, "inner.log");
                    FileInfo innerFile = new FileInfo(innerFilePath);
                    fileInitialTimes[innerFilePath] = innerFile.LastAccessTime;
                }
            }

            // Checking time changes and outputting results
            void CheckAndPrintChanges()
            {
                bool anyChanges = false;

                // Checking for file time changes
                foreach (var kvp in fileInitialTimes)
                {
                    FileInfo fi = new FileInfo(kvp.Key);
                    DateTime newTime = fi.LastAccessTime;

                    if (newTime != kvp.Value)
                    {
                        anyChanges = true;
                        Console.WriteLine($"File Changed: {Path.GetFileName(kvp.Key)}");
                        Console.WriteLine($"  Before: {kvp.Value}");
                        Console.WriteLine($"  After:  {newTime}");
                    }
                }

                //  Checking for Catalog Time Changes
                foreach (var kvp in dirInitialTimes)
                {
                    DirectoryInfo di = new DirectoryInfo(kvp.Key);
                    DateTime newTime = di.LastAccessTime;

                    if (newTime != kvp.Value)
                    {
                        anyChanges = true;
                        Console.WriteLine($"Directory Changed: {Path.GetFileName(kvp.Key)}");
                        Console.WriteLine($"  Before: {kvp.Value}");
                        Console.WriteLine($"  After:  {newTime}");
                    }
                }

                if (!anyChanges)
                {
                    Console.WriteLine("result:");
                    Console.WriteLine("No LastAccessTime changes detected.");
                }
            }

            // execute a test
            Console.WriteLine("Recording initial times...");
            RecordInitialTimes();

            Console.WriteLine("\nExecuting test function...");
            testFunction();

            Console.WriteLine("\nChecking for changes...");
            Console.WriteLine("result:");
            CheckAndPrintChanges();
        }

    }
}



//test code
//testLAT t1 = new testLAT();

////t1.CreateTestFolders();

//t1.CheckAccessTimeChanges(() => {
//    DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestRoot", "Test1"));
//    DateTime time = dirInfo.LastAccessTime;
//    Console.WriteLine($"Test1 access time: {time}");
//});

//Console.WriteLine("       ");
//Console.WriteLine("       ");
//Console.WriteLine("       ");
//Console.WriteLine("       ");
//t1.CheckAccessTimeChanges(() => {
//    string[] files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "TestRoot", "Test1"));
//});
