namespace ChocolateStoreCoreTests
{
    [ExcludeFromCodeCoverage]
    public class StringHelperTests
    {
        [Theory]
        [InlineData("'https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer.exe'", "https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer.exe", "bla", "123")]
        [InlineData("'https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer'", "https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer", "bla", "123")]
        [InlineData("\"https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer\"", "https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer", "bla", "123")]
        [InlineData("'https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer'", "https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer", "", "")]
        [InlineData("if (\"https://static.rust-lang.org/dist/2023-01-10/rust-mingw-1.66.1-i686-pc-windows-gnu.tar.gz\" -ne \"\") {", "https://static.rust-lang.org/dist/2023-01-10/rust-mingw-1.66.1-i686-pc-windows-gnu.tar.gz", "", "")]
        public void GetOriginalUrlPatterns(string originalUrl, string urlExpected, string id, string version)
        {
            // Arrange

            // Act
            var downloads = StringHelper.GetOriginalUrls(originalUrl, id, version, "###");

            // Assert
            downloads.Should().NotBeNull();
            downloads.Should().HaveCount(1);
            var download = downloads[0];
            download.Should().Be(urlExpected);
        }
    }
}
