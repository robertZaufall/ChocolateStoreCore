using NuGet.Versioning;

namespace ChocolateStoreCore.Models
{
    public interface IChocolateyPackage
    {
        string Id { get; set; }
        NuGetVersion Version { get; set; }
        NuGetVersion LatestVersion { get; set; }
        List<Dependency> Dependencies { get; set; }
        List<Dependency> LatestDependencies { get; set; }
        string FileName { get; }
        string DownloadUrl { get; set; }
    }

    public class ChocolateyPackage : IChocolateyPackage
    {
        public string Id { get; set; }
        public NuGetVersion Version { get; set; }
        public NuGetVersion LatestVersion { get; set; }
        public List<Dependency> Dependencies { get; set; }
        public List<Dependency> LatestDependencies { get; set; }
        public string FileName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.Id) && !string.IsNullOrWhiteSpace(this.Version.OriginalVersion))
                {
                    return $"{this.Id}.{this.Version.OriginalVersion}.nupkg";
                }
                return "";
            }
        }
        public string DownloadUrl { get; set; }
    }
}
