using NuGet.Versioning;

namespace ChocolateStoreCore.Models
{
    public interface IStorePackage
    {
        string FileName { get; set; }
        string Folder { get; set; }
        string Id { get; set; }
        string Path { get; set; }
        NuGetVersion Version { get; set; }
    }

    public class StorePackage : IStorePackage
    {
        public StorePackage()
        {
        }

        public string Path { get; set; }
        public string FileName { get; set; }
        public string Id { get; set; }
        public NuGetVersion Version { get; set; }
        public string Folder { get; set; }
    }
}
