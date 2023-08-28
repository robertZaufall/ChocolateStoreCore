using ChocolateStoreCore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Packaging;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace ChocolateStoreCore.Helpers
{
    public interface IChocolateyHelper
    {
        List<ChocolateyPackage> FlattenDependencies(List<ChocolateyPackage> chocolateyPackages);
        List<ChocolateyPackage> FlattenDependenciesSimple(List<ChocolateyPackage> chocolateyPackages);
        List<string> GetDownloadList(string path);
        ChocolateyPackage GetLastVersion(string id);
        List<Dependency> GetPackageDependenciesFromNuspec(NuspecReader nusprecReader);
        List<StorePackage> GetPackagesInventory(string path);
        ChocolateyPackage GetPackageWithDependenciesFromNupkgFile(MemoryStream stream);
        StorePackage ParseNupkg(MemoryStream stream);
        ChocolateyPackage ParseMetadata(string originalId, string content);
        (string, List<Download>, string) ExtractAndRewriteUrls(string content, string folder, string repo, string id, string version);
    }

    public class ChocolateyHelper : IChocolateyHelper
    {
        private readonly ILogger<ChocolateyHelper> _logger;
        private readonly IFileHelper _fileHelper;
        private readonly IHttpHelper _httpHelper;

        public ChocolateyHelper(IFileHelper fileHelper, IHttpHelper httpHelper, ILogger<ChocolateyHelper> logger)
        {
            _logger = logger ?? new Logger<ChocolateyHelper>(new NullLoggerFactory());
            _fileHelper = fileHelper;
            _httpHelper = httpHelper;
        }

        public (string, List<Download>, string) ExtractAndRewriteUrls(string contentOriginal, string folder, string repo, string id, string version)
        {
            var downloads = new List<Download>();
            var folderName = Path.GetFileName(folder);

            var content = StringHelper.ReplaceTokensByVariables(contentOriginal);

            var fileType = StringHelper.GetFileType(content);

            var originalUrls = StringHelper.GetOriginalUrls(content, id, version, notToReplaceUrl: repo);
            var transformedContent = Regex.Replace(content, StringHelper.RxUrlPattern.ToString(), new MatchEvaluator(m =>
            {
                if (string.IsNullOrEmpty(repo) || !m.Value.StartsWith(repo))
                {
                    var url = StringHelper.ReplaceTokens(m.Value, id, version);
                    var actualUrl = _httpHelper.CheckUrl(url);
                    var uri = new Uri(actualUrl ?? url);
                    var fileName = GetFileNameFromUrls(url, actualUrl);
                    var originalFileExtension = Path.GetExtension(fileName);

                    if (string.IsNullOrWhiteSpace(originalFileExtension) && !string.IsNullOrWhiteSpace(fileType))
                        fileName = fileName + "." + fileType;

                    var transformedUrl = repo + @"/" + folderName + @"/" + HttpUtility.UrlPathEncode(fileName);

                    downloads.Add(new Download { Url = url, Path = Path.Combine(folder, fileName) });
                    return transformedUrl;
                }
                return m.Value;
            }), RegexOptions.IgnoreCase);
            return (transformedContent, downloads, fileType);
        }

        public string GetFileNameFromUrls(string url, string actualUrl)
        {
            var actualUri = new Uri(actualUrl ?? url);
            var uri = new Uri(url);

            var fileName1 = HttpUtility.UrlDecode(Path.GetFileName((string.IsNullOrWhiteSpace(uri.Query) ? uri.OriginalString : uri.OriginalString.Replace(uri.Query, ""))));
            var fileName2 = HttpUtility.UrlDecode(Path.GetFileName((string.IsNullOrWhiteSpace(actualUri.Query) ? actualUri.OriginalString : actualUri.OriginalString.Replace(actualUri.Query, ""))));

            if (Path.HasExtension(fileName1) && Path.HasExtension(fileName2))
                return (fileName1.Length > fileName2.Length ? fileName2 : fileName1);

            if (!Path.HasExtension(fileName1) && !Path.HasExtension(fileName2))
                return (fileName1.Length > fileName2.Length ? fileName2 : fileName1);

            return (Path.HasExtension(fileName1) ? fileName1 : fileName2);
        }

        public List<string> GetDownloadList(string path)
        {
            if (!_fileHelper.FileExists(path))
            {
                _logger.LogError($"File {path} does not exist", path);
                throw new FileNotFoundException($"File {path} not found");
            }

            const string prefix_chocolatey = @"chocolatestore.exe https://chocolatey.org/api/v2/package/";
            const string prefix_comment = "rem ";

            return _fileHelper.GetListFromStream(path)
                .Select(x => x.Replace(prefix_chocolatey, "", StringComparison.OrdinalIgnoreCase).Replace("/", "").Replace("  ", " ").Trim())
                .Where(x => x.Length <= prefix_comment.Length || (x.Length > prefix_comment.Length && !x.Substring(0, prefix_comment.Length).Equals(prefix_comment, StringComparison.OrdinalIgnoreCase)))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        public StorePackage ParseNupkg(MemoryStream stream)
        {
            var package = new StorePackage();

            using (var nugetPackage = new PackageArchiveReader(stream))
            {
                var nuspecReader = nugetPackage.NuspecReader;
                package.Id = nuspecReader.GetId();
                package.Version = nuspecReader.GetVersion();
            }

            return package;
        }

        public ChocolateyPackage GetPackageWithDependenciesFromNupkgFile(MemoryStream stream)
        {
            ChocolateyPackage chocolateyPackage = null;

            using (var nugetPackage = new PackageArchiveReader(stream))
            {
                var nuspecReader = nugetPackage.NuspecReader;
                chocolateyPackage = new ChocolateyPackage
                {
                    Id = nuspecReader.GetId(),
                    Version = nuspecReader.GetVersion(),
                    Dependencies = GetPackageDependenciesFromNuspec(nuspecReader)
                };
            }

            return chocolateyPackage;
        }

        public List<Dependency> GetPackageDependenciesFromNuspec(NuspecReader nuspecReader)
        {
            if (nuspecReader == null)
                return null;

            var dependencies = new List<Dependency>();
            var dependencyGroups = nuspecReader.GetDependencyGroups();
            if (dependencyGroups != null && dependencyGroups.Any())
            {
                dependencyGroups.ToList().ForEach(x =>
                {
                    if (x.Packages != null && x.Packages.Any())
                    {
                        dependencies.AddRange(x.Packages.Select(y => new Dependency
                        {
                            Id = y.Id,
                            Version = (y.VersionRange.HasLowerBound ? y.VersionRange.MinVersion : null),
                            OriginalVersion = (y.VersionRange.HasLowerBound ? y.VersionRange.MinVersion.OriginalVersion : null),
                        }));
                    }
                });
            }
            return dependencies;
        }

        public List<StorePackage> GetPackagesInventory(string path)
        {
            var nupkgs = new List<StorePackage>();

            nupkgs = _fileHelper.GetNupkgFiles(path).Select(x =>
            {
                using (MemoryStream stream = _fileHelper.GetStream(x))
                {
                    var package = ParseNupkg(stream);
                    if (package != null)
                    {
                        package.Path = x;
                        package.FileName = Path.GetFileName(x);

                        var directoryName = Path.GetDirectoryName(package.Path);
                        if (directoryName != null)
                        {
                            var tempPath = Path.Combine(directoryName, $"{package.Id}.{package.Version.OriginalVersion}");
                            if (_fileHelper.DirectoryExists(tempPath))
                            {
                                package.Folder = tempPath;
                            }
                        }
                    }

                    return package;
                }
            }).ToList();

            return nupkgs;
        }

        public List<ChocolateyPackage> FlattenDependencies(List<ChocolateyPackage> chocolateyPackages)
        {
            var flattenedDependencies = new List<ChocolateyPackage>();
            var temp = chocolateyPackages.Select(x => new ChocolateyPackage { Id = x.Id, Version = x.Version, DownloadUrl = x.DownloadUrl, Dependencies = x.Dependencies }).ToList();

            int iteration = 0;
            List<ChocolateyPackage> result;
            do
            {
                result = FlattenDependenciesSimple(temp);
                result.ForEach(x =>
                {
                    if (!flattenedDependencies.Exists(y => y.Id == x.Id))
                    {
                        flattenedDependencies.Add(new ChocolateyPackage
                        {
                            Id = x.Id,
                            Version = x.Version,
                            DownloadUrl = x.DownloadUrl,
                            Dependencies = x.Dependencies
                        });
                    }
                });

                List<Dependency> dependencies = new();
                result.ForEach(x =>
                {
                    if (x.Dependencies != null && x.Dependencies.Count > 0)
                    {
                        dependencies.AddRange(x.Dependencies);
                    }
                });
                temp = dependencies.Select(x => GetLastVersion(x.Id)).ToList();

                iteration++;
            } while (temp.Count > 0 && iteration < 10);

            return flattenedDependencies;
        }

        public List<ChocolateyPackage> FlattenDependenciesSimple(List<ChocolateyPackage> chocolateyPackages)
        {
            var result = new List<ChocolateyPackage>();
            chocolateyPackages.ForEach(x =>
            {
                if (x.Dependencies != null)
                {
                    x.Dependencies.ForEach(y =>
                    {
                        if (!result.Exists(a => a.Id == y.Id))
                        {
                            var lastVersion = GetLastVersion(y.Id);
                            result.Add(lastVersion);
                        }
                    });
                    x.Dependencies = null;
                }
                result.Add(x);
            });

            return result.DistinctBy(x => x.Id).ToList();
        }

        public ChocolateyPackage GetLastVersion(string id)
        {
            var content = _httpHelper.GetMetadataForPackageId(id);
            return ParseMetadata(id, content);
        }

        public ChocolateyPackage ParseMetadata(string originalId, string content)
        {
            try
            {
                var xml = XElement.Parse(content);

                XNamespace m = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
                XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
                XNamespace ns = "http://www.w3.org/2005/Atom";

                var id = xml.Descendants(ns + "entry").Select(x => (string)x.Element(ns + "title")).FirstOrDefault();

                if (id != null)
                {
                    var version = xml.Descendants(m + "properties").Select(x => (string)x.Element(d + "Version")).FirstOrDefault();
                    var downloadUrl = xml.Descendants(ns + "entry").Select(x => x.Element(ns + "content")).Select(x => x?.Attribute("src")?.Value).FirstOrDefault();
                    var xDependencies = xml.Descendants(m + "properties").Select(x => (string)x.Element(d + "Dependencies")).FirstOrDefault();
                    List<Dependency> dependencies = xDependencies?
                        .Split("|")
                        .ToList()
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(y =>
                        {
                            var depId = y?.Split(":")[0];
                            var parsed = VersionRange.TryParse(y?.Split(":")[1], out var depVersionRange);
                            var depVersion = parsed ? (
                                depVersionRange.HasLowerBound ?
                                    depVersionRange.MinVersion.OriginalVersion :
                                    depVersionRange.MaxVersion.OriginalVersion) :
                                null;
                            return new Dependency
                            {
                                Id = depId,
                                OriginalVersion = depVersion,
                                Version = (depVersion == null ? null : new NuGetVersion(depVersion)),
                            };
                        }).ToList();

                    return new ChocolateyPackage
                    {
                        Id = id,
                        Version = new NuGetVersion(version),
                        Dependencies = dependencies,
                        DownloadUrl = downloadUrl,
                    };
                }
                else
                {
                    return new ChocolateyPackage { Id = originalId };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing metadata for '{originalId}'", originalId);
                return null;
            }
        }
    }
}
