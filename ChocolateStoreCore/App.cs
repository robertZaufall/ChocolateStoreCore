using ChocolateStoreCore.Helpers;
using ChocolateStoreCore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;

namespace ChocolateStoreCore
{
    public interface IApp
    {
        Task<int> Run(bool whatif);
        Task<int> Purge(bool whatif);
    }

    public class App : IApp
    {
        private readonly ILogger<IApp> _logger;
        private readonly ISettings _settings;
        private readonly IPackageCacher _packageCacher;
        private readonly IChocolateyHelper _chocolateyHelper;
        private readonly IFileHelper _fileHelper;

        public App(ILogger<IApp> logger, ISettings settings, IChocolateyHelper chocolateyHelper, IPackageCacher packageCacher, IFileHelper fileHelper)
        {
            _logger = logger ?? new Logger<IApp>(new NullLoggerFactory());
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _packageCacher = packageCacher;
            _fileHelper = fileHelper;
            _chocolateyHelper = chocolateyHelper;
        }

        public async Task<int> Run(bool whatif = false)
        {
            var sourcePath = _settings.FolderPath;
            var targetPath = !whatif ? sourcePath : Path.GetTempPath();
            
            if (!_fileHelper.DirectoryExists(targetPath))
                _fileHelper.DirectoryCreateDirectory(targetPath);

            await Purge(whatif).ConfigureAwait(false);

            var downloads = _chocolateyHelper.GetDownloadList(_settings.DownloadListPath);
            _logger.LogInformation("found {downloads_Count} packages.", downloads.Count);

            var neededPackages = _packageCacher.GetLastVersions(downloads, flattenDependencies: true);
            
            var existingIds = neededPackages.Select(x => x.Id).ToList();
            var nupkgs = _chocolateyHelper.GetPackagesInventory(sourcePath);

            neededPackages.AddRange(
                nupkgs.Where(x => !existingIds.Contains(x.Id))
                      .Select(x => new ChocolateyPackage { Id = x.Id, Version = x.Version }).ToList());

            neededPackages.ForEach(x =>
            {
                _ = _packageCacher.CachePackage(x, sourcePath, targetPath, whatif);
            });
            
            await Task.CompletedTask;
            _logger.LogInformation("Run done.");

            return 0;
        }

        public async Task<int> Purge(bool whatif = false)
        {
            var sourcePath = _settings.FolderPath;
            var targetPath = !whatif ? sourcePath : Path.GetTempPath();

            if (!_fileHelper.DirectoryExists(sourcePath))
                throw new ArgumentOutOfRangeException(nameof(sourcePath), $"Path {sourcePath} does not exist");

            if (!_fileHelper.DirectoryExists(targetPath))
                _fileHelper.DirectoryCreateDirectory(targetPath);

            _logger.LogWarning("Start purging {path}...", sourcePath);
            _packageCacher.Purge(sourcePath, whatif);

            await Task.CompletedTask;
            _logger.LogWarning("Purging {path} done.", sourcePath);

            return 0;
        }
    }
}
