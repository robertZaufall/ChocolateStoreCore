namespace ChocolateStoreCore.Models
{
    public interface IDownload
    {
        string Url { get; set; }
        string Path { get; set; }
    }

    public class Download : IDownload
    {
        public string Url { get; set; }
        public string Path { get; set; }
    }
}
