using System;
using System.IO;
using System.Collections.Concurrent;

public class FileMonitor
{
    private FileSystemWatcher watcher;
    private ConcurrentDictionary<string, DateTime> fileChanges = new ConcurrentDictionary<string, DateTime>();

    public FileMonitor(string path)
    {
        // 创建文件系统监视器
        watcher = new FileSystemWatcher(path);

        // 设置要监视的更改类型
        watcher.NotifyFilter = NotifyFilters.Attributes
                            | NotifyFilters.CreationTime
                            | NotifyFilters.DirectoryName
                            | NotifyFilters.FileName
                            | NotifyFilters.LastAccess
                            | NotifyFilters.LastWrite
                            | NotifyFilters.Security
                            | NotifyFilters.Size;

        // 是否监视子目录
        watcher.IncludeSubdirectories = true;

        // 注册事件处理器
        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;

        // 启动监视
        watcher.EnableRaisingEvents = true;
    }

    // 文件被修改时触发
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }
        fileChanges.AddOrUpdate(e.FullPath, DateTime.Now, (key, oldValue) => DateTime.Now);
        Console.WriteLine($"文件已更改: {e.FullPath}");
    }

    // 新建文件时触发
    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        fileChanges.AddOrUpdate(e.FullPath, DateTime.Now, (key, oldValue) => DateTime.Now);
        Console.WriteLine($"新建文件: {e.FullPath}");
    }

    // 删除文件时触发
    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        fileChanges.TryRemove(e.FullPath, out _);
        Console.WriteLine($"删除文件: {e.FullPath}");
    }

    // 重命名文件时触发
    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        fileChanges.TryRemove(e.OldFullPath, out _);
        fileChanges.AddOrUpdate(e.FullPath, DateTime.Now, (key, oldValue) => DateTime.Now);
        Console.WriteLine($"重命名文件: {e.OldFullPath} -> {e.FullPath}");
    }

    // 获取指定时间后更改的文件
    public List<string> GetRecentChangedFiles(DateTime since)
    {
        return fileChanges
            .Where(kvp => kvp.Value >= since)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    // 停止监视
    public void Stop()
    {
        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
    }
}

// 使用示例
public class Program
{
    public static void Main()
    {
        string path = @"C:\监视目录";
        var monitor = new FileMonitor(path);

        // 示例：获取过去1小时内更改的文件
        var recentFiles = monitor.GetRecentChangedFiles(DateTime.Now.AddHours(-1));

        // 程序结束时
        monitor.Stop();
    }
}