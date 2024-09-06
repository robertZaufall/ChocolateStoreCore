using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace ChocolateStoreCore.Helpers
{
    public interface IFileHelper
    {
        bool FileExists(string path);
        bool DirectoryExists(string path);
        bool DirectoryCreateDirectory(string path);
        bool FileDelete(string path);
        bool DirectoryDelete(string path);
        List<string> GetListFromStream(string path);
        List<string> GetDirectoryNames(string path);
        List<string> GetNupkgFiles(string path);
        MemoryStream GetStream(string path);
        Dictionary<string, string> GetContentFromZip(string path, string fileName);
        bool UpdateContentInZip(string path, string fileName, string content);
        bool WriteDummyFile(string path);
        bool FileCreate(string path, Stream stream);
        bool FileCopy(string sourcePath, string targetPath);
    }

    [ExcludeFromCodeCoverage]
    public class FileHelper : IFileHelper
    {
        public static string GetApplicationRoot() => Path.GetDirectoryName(AppContext.BaseDirectory);
        //public static string GetApplicationRoot() => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public bool FileExists(string path) => File.Exists(path);
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public bool DirectoryCreateDirectory(string path) => Directory.Exists(path) ? true : Directory.CreateDirectory(path).Exists;
        public bool DirectoryDelete(string path)
        {
            if (Directory.Exists(path))
            {
                RemoveReadOnlyAttribute(path);
                Directory.Delete(path, true);
            }
            return true;
        }
        public bool FileDelete(string path)
        {
            if (File.Exists(path))
            {
                RemoveReadOnlyAttribute(path);
                File.Delete(path);
            }
            return true;
        }
        public List<string> GetListFromStream(string path) => new StreamReader(path).ReadToEnd().Split("\r\n").ToList();
        public List<string> GetDirectoryNames(string path) => Directory.GetDirectories(path).Select(x => Path.GetFileName(x)).ToList();
        public List<string> GetNupkgFiles(string path) => Directory.EnumerateFiles(path, "*.nupkg").ToList();
        public MemoryStream GetStream(string path)
        {
            return new MemoryStream(File.ReadAllBytes(path));
        }
        public Dictionary<string, string> GetContentFromZip(string path, string filePattern)
        {
            var contents = new Dictionary<string, string>();
            using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Read))
            {
                var regex = new System.Text.RegularExpressions.Regex(filePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var entries = archive.Entries.Where(x => regex.IsMatch(x.FullName)).ToList();

                foreach (var entry in entries)
                {
                    using var reader = new StreamReader(entry.Open());
                    contents.Add(entry.FullName, reader.ReadToEnd());
                }
            }
            return contents;
        }

        public bool UpdateContentInZip(string path, string fileName, string content)
        {
            using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Update))
            {
                var entry = archive.Entries.Where(x => x.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (entry != null)
                {
                    entry.Delete();

                    var newEntry = archive.CreateEntry(fileName, CompressionLevel.Optimal);

                    using (StreamWriter writer = new StreamWriter(newEntry.Open()))
                    {
                        writer.BaseStream.Seek(0, SeekOrigin.Begin);
                        writer.WriteLine(content);
                    }
                    newEntry.LastWriteTime = DateTimeOffset.UtcNow.LocalDateTime;
                    return true;
                }
            }
            return false;
        }

        public bool WriteDummyFile(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine(Path.GetFileName(path));
            }
            return true;
        }

        public bool FileCreate(string path, Stream stream)
        {
            using (var fs = File.Create(path))
            {
                stream.CopyTo(fs);
            }
            return true;
        }

        public bool FileCopy(string sourcePath, string targetPath)
        {
            File.Copy(sourcePath, targetPath, true);
            return true;
        }

        private void RemoveReadOnlyAttribute(string path)
        {
            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false;
                }
            }
            else if (Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                foreach (FileInfo file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    if (file.IsReadOnly)
                    {
                        file.IsReadOnly = false;
                    }
                }
                foreach (DirectoryInfo dir in directoryInfo.GetDirectories("*", SearchOption.AllDirectories))
                {
                    if ((dir.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        dir.Attributes &= ~FileAttributes.ReadOnly;
                    }
                }
            }
        }

    }
}
