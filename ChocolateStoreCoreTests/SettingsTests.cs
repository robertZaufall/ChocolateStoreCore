using Microsoft.Extensions.Configuration;

namespace ChocolateStoreCoreTests
{
    [ExcludeFromCodeCoverage]
    public class SettingsTests
    {
        public SettingsTests()
        {
        }

        [Fact]
        public void InstantiationTest()
        {
            // Arrange
            var myConfiguration = new Dictionary<string, string>
            {
                {"ChocolateyConfiguration:LocalRepoUrl", "1"},
                {"ChocolateyConfiguration:ApiUrl", "2"},
                {"ChocolateyConfiguration:ApiUserAgent", "3"},
                {"ChocolateyConfiguration:ApiPath", "4"},
                {"ChocolateyConfiguration:ApiPackageRequest", "5"},
                {"ChocolateyConfiguration:ApiPackageRequestWithVersion", "6"},
                {"ChocolateyConfiguration:ApiFindAllRequest", "7"},
                {"ChocolateyConfiguration:ApiFindAllNextRequest", "8"},
                {"ChocolateyConfiguration:ApiGetRequest", "9"},
                {"ChocolateyConfiguration:OptionalRemoteDownloadUrl", "10"},
                {"ChocolateyConfiguration:FolderPath", "11"},
                {"ChocolateyConfiguration:DownloadListPath", "12"},
                {"ChocolateyConfiguration:HttpTimeout", "13"},
                {"ChocolateyConfiguration:HttpRetries", "14"},
                {"ChocolateyConfiguration:HttpRetrySleep", "18"},
                {"ChocolateyConfiguration:HttpDelay", "21"},
                {"ChocolateyConfiguration:HttpHandlerLifetime", "15"},
                {"ChocolateyConfiguration:LogFile", "16"},
                {"ChocolateyConfiguration:HttpTimeoutOverAll", "17"},
                {"ChocolateyConfiguration:LogLevel", "19"},
                {"ChocolateyConfiguration:FolderDelimiter", "20"},
                {"ChocolateyConfiguration:InstallFilesPattern", "22"},
                {"ChocolateyConfiguration:AdditionalPurgeOfFolders", "true"},
            };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(myConfiguration).Build();

            // Act
            var settings = new Settings(configuration, "");

            // Assert
            settings.Should().NotBeNull();
            settings.LocalRepoUrl.Should().Be("1");
            settings.ApiUrl.Should().Be("2");
            settings.ApiUserAgent.Should().Be("3");
            settings.ApiPath.Should().Be("4");
            settings.ApiPackageRequest.Should().Be("5");
            settings.ApiPackageRequestWithVersion.Should().Be("6");
            settings.ApiFindAllRequest.Should().Be("7");
            settings.ApiFindAllNextRequest.Should().Be("8");
            settings.ApiGetRequest.Should().Be("9");
            settings.OptionalRemoteDownloadUrl.Should().Be("10");
            settings.FolderPath.Should().Be("11");
            settings.DownloadListPath.Should().Be("12");
            settings.HttpTimeout.Should().Be(13);
            settings.HttpRetries.Should().Be(14);
            settings.HttpRetrySleep.Should().Be(18);
            settings.HttpDelay.Should().Be(21);
            settings.HttpHandlerLifetime.Should().Be(15);
            settings.LogFile.Should().Be("16");
            settings.HttpTimeoutOverAll.Should().Be(17);
            settings.LogLevel.Should().Be("19");
            settings.FolderDelimiter.Should().Be("20");
            settings.InstallFilesPattern.Should().Be("22");
            settings.AdditionalPurgeOfFolders.Should().BeTrue();
        }

        [Fact]
        public void InstantiationDefaultsTest()
        {
            // Arrange
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            // Act
            var settings = new Settings(configuration, "");

            // Assert
            settings.Should().NotBeNull();
            settings.HttpRetrySleep.Should().Be(30);
            settings.HttpDelay.Should().Be(0);
            settings.FolderDelimiter.Should().Be(".");
            settings.InstallFilesPattern.Should().Be("tools/(ChocolateyInstall\\.ps|data\\.ps)");
        }
    }
}
