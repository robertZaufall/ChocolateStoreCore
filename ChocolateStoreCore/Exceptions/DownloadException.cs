namespace ChocolateStoreCore.Exceptions
{
    public class DownloadException : Exception
    {
        public DownloadException()
        {
        }

        public DownloadException(string message)
            : base(message)
        {
        }

        public DownloadException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
