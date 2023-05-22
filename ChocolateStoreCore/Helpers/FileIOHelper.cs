using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;

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
        string GetContentFromZip(string path, string fileName);
        bool UpdateContentInZip(string path, string fileName, string content);
        bool WriteDummyFile(string path);
        bool FileCreate(string path, Stream stream);
    }

    [ExcludeFromCodeCoverage]
    public class FileHelper : IFileHelper
    {
        public string GetApplicationRoot() => Path.GetDirectoryName(AppContext.BaseDirectory);

        public bool FileExists(string path) => File.Exists(path);
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public bool DirectoryCreateDirectory(string path) => Directory.Exists(path) ? true : Directory.CreateDirectory(path).Exists;
        public bool DirectoryDelete(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            return true;
        }
        public bool FileDelete(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            return true;
        }
        public List<string> GetListFromStream(string path) => new StreamReader(path).ReadToEnd().Split("\r\n").ToList();
        public List<string> GetDirectoryNames(string path) => Directory.GetDirectories(path).Select(x => Path.GetFileName(x)).ToList();
        public List<string> GetNupkgFiles(string path) => Directory.EnumerateFiles(path, "*.nupkg").ToList();
        public MemoryStream GetStream(string path)
        {
            return new MemoryStream(File.ReadAllBytes(path));
        }
        public string GetContentFromZip(string path, string fileName)
        {
            using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Read))
            {
                var entry = archive.Entries.Where(x => x.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (entry != null)
                {
                    using var reader = new StreamReader(entry.Open());
                    return reader.ReadToEnd();
                }
            }
            return null;
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
    }
}
