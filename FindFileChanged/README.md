
## 管理员权限的危险性
  当授予一个应用程序管理员权限后，这个程序就获得了很高的系统权限，它可以访问和修改系统中的绝大多数文件（包括其他应用程序的文件）、修改注册表、安装服务等，这就像是你把家里的钥匙交给了一个陌生人，虽然系统还有一些保护机制（如TrustedInstaller保护和内存空间隔离），但总体来说安全风险很高。


## 1.
一开始，我想这存储程序运行之前的文件的快照，然后对比，但这样的开销是巨大的。
## 2. 
然后，我想这通过遍历来查看文件以及文件夹被打开的时间（利用windows的Last Access Time功能），于是有了“RecentlyOpenedFile.cs”。在写代码的时候我遇见了一个问题：当我用函数遍历文件夹里文件时候，几百年是没有打开这个文件，文件的Last Access Time依然会被更改。
//     
  DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
  dirInfo.LastAccessTime  // 不会触发更新

  FileInfo fileInfo = new FileInfo(filePath);
  fileInfo.LastAccessTime  // 不会触发更新

  Directory.GetLastAccessTime()//会触发目录的访问时间更新
  File.GetLastAccessTime()//会触发目录的访问时间更新
  FileInfo.GetLastAccessTime()//会触发目录的访问时间更新
//


Windows系统默认会记录文件的访问时间，即使只是查看文件属性而没有实际打开文件内容。
几个主要原因：

1） Windows默认启用了"最后访问时间"(Last Access Time)的更新功能，查看文件属性也被视为一种"访问"

2） 以下操作都会更新访问时间：
   - 查看文件属性
   - 文件资源管理器预览
   - 索引服务扫描
   - 防病毒软件扫描
   - 备份程序访问

如果想禁用这个功能，可以：

1. 通过注册表:
 HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem
 将NtfsDisableLastAccessUpdate值设为1

2. 或使用管理员权限在命令行执行:
  fsutil behavior set disablelastaccess 1

这样可以提高系统性能，因为系统不用频繁更新文件的访问时间。
## 3.
因此尝试使用文件系统监视器（更好的替代方案）“FileMonitor.cs”
## 4.
“testLAT.cs”用于测试某个函数是否更改了访问时间。
