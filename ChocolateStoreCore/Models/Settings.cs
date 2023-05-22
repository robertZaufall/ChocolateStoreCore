using Microsoft.Extensions.Configuration;
using Serilog.Events;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace ChocolateStoreCore.Models
{
    public interface ISettings
    {
        string ApiFindAllNextRequest { get; set; }
        string ApiFindAllRequest { get; set; }
        string ApiGetRequest { get; set; }
        string ApiPackageRequest { get; set; }
        string ApiPackageRequestWithVersion { get; set; }
        string ApiPath { get; set; }
        string ApiUrl { get; set; }
        string ApiUserAgent { get; set; }
        string FolderPath { get; set; }
        string LocalRepoUrl { get; set; }
        string OptionalRemoteDownloadUrl { get; set; }
        string DownloadListPath { get; set; }
        int HttpTimeout { get; set; }
        int HttpTimeoutOverAll { get; set; }
        int HttpRetries { get; set; }
        int HttpHandlerLifetime { get; set; }
        string LogFile { get; set; }
        string LogLevel { get; set; }
        LogEventLevel GetLogLevel();
    }

    public class Settings : ISettings
    {
        public Settings()
        {
        }

        [RequiresUnreferencedCode("Dynamic configuration binding.")]
        public Settings(IConfiguration config)
        {
            try
            {
                config?.Bind("ChocolateyConfiguration", this);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static IConfiguration GetConfiguration()
        {
            var parentDirectory = Directory.GetParent(AppContext.BaseDirectory);
            if (parentDirectory != null)
                return new ConfigurationBuilder().SetBasePath(parentDirectory.FullName).AddJsonFile("appsettings.json", false).Build();
            return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();
        }

        public string LocalRepoUrl { get; set; }
        public string ApiUrl { get; set; }
        public string ApiUserAgent { get; set; }
        public string ApiPath { get; set; }
        public string ApiPackageRequest { get; set; }
        public string ApiPackageRequestWithVersion { get; set; }
        public string ApiFindAllRequest { get; set; }
        public string ApiFindAllNextRequest { get; set; }
        public string ApiGetRequest { get; set; }
        public string OptionalRemoteDownloadUrl { get; set; }
        public string FolderPath { get; set; }
        public string DownloadListPath { get; set; }
        public int HttpTimeout { get; set; }
        public int HttpTimeoutOverAll { get; set; }
        public int HttpRetries { get; set; }
        public int HttpHandlerLifetime { get; set; }
        public string LogFile { get; set; }
        public string LogLevel { get; set; }

        public LogEventLevel GetLogLevel()
        {
            LogEventLevel level = LogEventLevel.Information;
            Enum.TryParse<LogEventLevel>(LogLevel, true, out level);
            return level;
        }
    }
}
