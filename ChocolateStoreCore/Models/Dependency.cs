using NuGet.Versioning;

namespace ChocolateStoreCore.Models
{
    public interface IDependency
    {
        string Id { get; set; }
        NuGetVersion Version { get; set; }
        string OriginalVersion { get; set; }
    }

    public class Dependency : IDependency
    {
        public string Id { get; set; }
        public NuGetVersion Version { get; set; }
        public string OriginalVersion { get; set; }
    }
}
