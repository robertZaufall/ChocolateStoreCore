﻿using ChocolateStoreCore.Exceptions;
using ChocolateStoreCore.Helpers;
using ChocolateStoreCore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChocolateStoreCore
{
    public interface IPackageCacher
    {
        bool CachePackage(ChocolateyPackage package, string sourcePath, string targetPath = null, bool whatif = false);
        List<ChocolateyPackage> GetLastVersions(List<string> downloads, bool flattenDependencies);
        List<ChocolateyPackage> GetMissingFromDownloadsAndDependencies(List<string> downloads, string path, bool localOnly);
        bool PurgeVsCodeExtensionsFolders(string path = null, bool whatif = false);
        bool Purge(string path = null, bool whatif = false);
        List<string> GetInventoryList(string path);
    }

    public class PackageCacher : IPackageCacher
    {
        private readonly ISettings _settings;
        private readonly ILogger<PackageCacher> _logger;
        private readonly IFileHelper _fileHelper;
        private readonly IHttpHelper _httpHelper;
        private readonly IChocolateyHelper _chocolateyHelper;

        public PackageCacher(ISettings settings, IFileHelper fileHelper, IHttpHelper httpHelper, IChocolateyHelper chocolateyHelper, ILogger<PackageCacher> logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _httpHelper = httpHelper;
            _fileHelper = fileHelper;
            _chocolateyHelper = chocolateyHelper;
            _logger = logger ?? new Logger<PackageCacher>(new NullLoggerFactory());
        }

        public List<string> GetInventoryList(string path)
        {
            var inventory = _chocolateyHelper.GetPackagesInventory(path);
            return inventory?.Select(x => x.Id).ToList();
        }

        public bool Purge(string path = null, bool whatif = false)
        {
            var folders = _fileHelper.GetDirectoryNames(path);
            var inventory = _chocolateyHelper.GetPackagesInventory(path);
            _logger.LogInformation("Found {folders_Count} package folders and {inventory_Count} nupkgs files.", folders?.Count, inventory?.Count);

            DoPurge(inventory, whatif);

            if (_settings.AdditionalPurgeOfFolders)
            {
                var foldersToPurge = _chocolateyHelper.GetDirectoriesOnlyInventory(path, _settings.FolderDelimiter);
                _logger.LogInformation("Found {foldersToPurge_Count} folders for additional purging.", foldersToPurge?.Count);

                DoPurge(foldersToPurge, whatif);
            }
            
            return true;
        }

        public bool PurgeVsCodeExtensionsFolders(string path = null, bool whatif = false)
        {
            var inventory = _chocolateyHelper.GetDirectoriesOnlyInventory(path, "-");
            _logger.LogInformation("Found {inventory_Count} VSCode extension folders.", inventory?.Count);

            return DoPurge(inventory, whatif);
        }

        private bool DoPurge(List<StorePackage> inventory, bool whatif = false)
        {
            var toPurge = inventory?.GroupBy(
                x => x.Id,
                x => x.Version,
                (baseid, versions) => new
                {
                    Key = baseid,
                    Max = versions.Max(),
                    Count = versions.Count(),
                    Versions = versions
                        .Where(x =>
                            x != versions.Max() ||
                            x == versions.Max() && versions.Count(v => v == x) >= 2 && x.OriginalVersion?.Length == versions.Min(v => v.OriginalVersion?.Length)
                        ).ToList()
                }).ToList();

            var toReallyPurge = toPurge?.Where(x => x.Count > 1).OrderBy(x => x.Count).ToList();

            _logger.LogWarning("{toReallyPurge_Count} groups for purging.", toReallyPurge?.Count);

            toPurge?.ForEach(x => x.Versions.ForEach(y =>
            {
                _logger.LogInformation("Processing {x_Key}, {y_Version}", x.Key, y.Version);

                var todo = inventory?.Where(a => a.Id == x.Key && a.Version == y)
                    .Select(a => new { a.Path, a.Folder })
                    .FirstOrDefault();

                if (todo != null)
                {
                    if (_fileHelper.DirectoryExists(todo.Folder))
                    {
                        _logger.LogWarning("Deleting folder {todo_Folder}", todo.Folder);
                        if (!whatif)
                            _fileHelper.DirectoryDelete(todo.Folder);
                    }

                    if (!string.IsNullOrWhiteSpace(todo.Path) && _fileHelper.FileExists(todo.Path))
                    {
                        _logger.LogWarning("Deleting file {todo_Path}", todo.Path);
                        if (!whatif)
                            _fileHelper.FileDelete(todo.Path);
                    }
                }
            }));

            return true;
        }

        public List<ChocolateyPackage> GetMissingFromDownloadsAndDependencies(List<string> downloads, string path, bool localOnly)
        {
            var missingChocolateyPackages = new List<ChocolateyPackage>();

            var nupkgs = _chocolateyHelper.GetPackagesInventory(path);
            _logger.LogInformation("Found {nupkgs_Count} nupkgs files.", nupkgs?.Count);

            var dependenciesList = new List<ChocolateyPackage>();

            nupkgs?.ForEach(x =>
            {
                var stream = _fileHelper.GetStream(x.Path);
                dependenciesList.Add(_chocolateyHelper.GetPackageWithDependenciesFromNupkgFile(stream));
            });

            dependenciesList.ForEach(x => x.Dependencies?.ForEach(z =>
            {
                if (nupkgs != null && !nupkgs.Exists(a => a.Id == z.Id))
                    if (!missingChocolateyPackages.Exists(a => a.Id == z.Id))
                        missingChocolateyPackages.Add(new ChocolateyPackage { Id = z.Id });
            }));

            downloads.ForEach(x =>
            {
                if (nupkgs != null && !nupkgs.Exists(a => a.Id == x))
                    if (!missingChocolateyPackages.Exists(b => b.Id == x))
                        missingChocolateyPackages.Add(new ChocolateyPackage { Id = x });
            });

            return missingChocolateyPackages;
        }

        public List<ChocolateyPackage> GetLastVersions(List<string> downloads, bool flattenDependencies)
        {
            var chocolateyPackages = downloads.Select(x =>
            {
                return _chocolateyHelper.GetLastVersion(x.ToLowerInvariant());
            }).Where(x => x != null).ToList();
            return !flattenDependencies ? chocolateyPackages : _chocolateyHelper.FlattenDependencies(chocolateyPackages);
        }

        public bool CachePackage(ChocolateyPackage package, string sourcePath, string targetPath = null, bool whatif = false)
        {
            bool waitAfterDownload = false;
            try
            {
                var sourcePackagePath = Path.Combine(sourcePath, package.FileName);
                var targetPackagePath = Path.Combine(targetPath ?? sourcePath, package.FileName);

                if (!whatif && !_fileHelper.FileExists(sourcePackagePath))
                {
                    _httpHelper.DownloadFile(package.DownloadUrl, targetPackagePath);

                    _logger.LogInformation($"Download done {package.FileName}.");
                    if (_settings.HttpDelay > 0)
                        waitAfterDownload = true;
                }
                if (whatif)
                {
                    _fileHelper.FileCopy(sourcePackagePath, targetPackagePath);
                }

                string folderName = $"{package.Id}{_settings.FolderDelimiter}{package.Version.Version}";
                string folder = Path.Combine(targetPath ?? sourcePath, folderName);

                try
                {
                    var filesWithContent = _fileHelper.GetContentFromZip(targetPackagePath, _settings.InstallFilesPattern);

                    foreach (var fileWithContent in filesWithContent)
                    {
                        var content = fileWithContent.Value;
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            (var contentNew, var downloads, _) = _chocolateyHelper.ExtractAndRewriteUrls(content, folder, _settings.LocalRepoUrl, package.Id, package.Version.OriginalVersion);

                            if (!string.IsNullOrWhiteSpace(contentNew) && downloads.Count > 0 && !_fileHelper.DirectoryExists(folder))
                            {
                                if (_fileHelper.DirectoryCreateDirectory(folder))
                                {
                                    downloads.ForEach(x =>
                                    {
                                        _logger.LogWarning("Downloading {x_Url}", x.Url);
                                        if (!whatif && !_fileHelper.FileExists(x.Path))
                                        {
                                            var returnPath = _httpHelper.DownloadFile(x.Url, x.Path);
                                            if (returnPath == null || !_fileHelper.FileExists(returnPath))
                                                throw new DownloadException(string.Format("Download not successful: {0}", x.Url));
                                        }
                                        if (whatif)
                                            _fileHelper.WriteDummyFile(x.Path);
                                    });
                                    return _fileHelper.UpdateContentInZip(targetPackagePath, fileWithContent.Key, contentNew);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while caching package {package_Id} {package_Version}", package.Id, package.Version.OriginalVersion);

                    try
                    {
                        _fileHelper.DirectoryDelete(folder);
                        _fileHelper.FileDelete(targetPackagePath);
                    }
                    catch
                    {
                        _logger.LogError(ex, "Error while cleaning failed package and folder for {package_Id} {package_Version}", package.Id, package.Version.OriginalVersion);
                    }

                    return false;
                }

                if (waitAfterDownload)
                {
                    _logger.LogWarning($"Delayed processing after package download ({package.FileName}) for {_settings.HttpDelay} seconds.");
                    Thread.Sleep(TimeSpan.FromSeconds(_settings.HttpDelay));
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching package {package_Id}", package.Id);
                return false;
            }
        }
    }
}