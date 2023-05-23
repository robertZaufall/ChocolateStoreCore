using ChocolateStoreCore;
using ChocolateStoreCore.Models;
using System.Net;

namespace ChocolateyStoreCoreTestsE2E
{
    [ExcludeFromCodeCoverage]
    public class PackageCacherTests : IClassFixture<TestFixture>
    {
        readonly TestFixture _fixture;

        public PackageCacherTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ScanAllNupkgFiles()
        {
            // Arrange
            var tempPath = _fixture.GetTemp();
            Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Resources", "nupkg")).ToList().ForEach(x => { File.Copy(x, Path.Combine(tempPath, Path.GetFileName(x))); });

            var dependenciesList = new List<ChocolateyPackage>();
            var dependencySpecialList = new List<Dependency>();

            // Act
            var files = Directory.GetFiles(tempPath, "*.nupkg").ToList();
            files.ForEach(x => {
                var stream = _fixture.FileHelper.GetStream(x);
                dependenciesList.Add(_fixture.ChocolateyHelper.GetPackageWithDependenciesFromNupkgFile(stream));
            });

            dependenciesList.ForEach(x => x.Dependencies?.ToList()
            .ForEach(z =>
            {
                if (!dependencySpecialList.Exists(a => a.Id == z.Id))
                {
                    dependencySpecialList.Add(z);
                }
            }));

            // Assert
            dependenciesList.Should().HaveCount(13);
            dependencySpecialList.Should().HaveCount(6);
        }

        [Fact]
        public void PurgeFiles()
        {
            // Arrange
            var tempPath = _fixture.GetTemp();
            Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Resources", "nupkg")).ToList().ForEach(x => { File.Copy(x, Path.Combine(tempPath, Path.GetFileName(x))); });

            var sut = new PackageCacher(_fixture.Settings, _fixture.FileHelper, _fixture.HttpHelper, _fixture.ChocolateyHelper, null);

            // Act
            sut.Purge(tempPath);

            // Assert
            var result = Directory.EnumerateFiles(tempPath).ToList();
            result.Should().HaveCount(9);
            result.Should().Contain(x => Path.GetFileName(x).Equals("vscode.install.1.69.0.nupkg", StringComparison.OrdinalIgnoreCase));
            result.Should().NotContain(x => Path.GetFileName(x).Equals("vscode.install.1.68.1.nupkg", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void PurgeFilesAndFolders()
        {
            // Arrange
            var tempPath = _fixture.GetTemp();
            Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Resources", "nupkg")).ToList().ForEach(x => { File.Copy(x, Path.Combine(tempPath, Path.GetFileName(x))); });
            Directory.EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Resources", "nupkg")).ToList().ForEach(x => { Directory.CreateDirectory(Path.Combine(tempPath, Path.GetFileName(x))); });
            Directory.EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Resources", "nupkg")).ToList().ForEach(x => { Directory.EnumerateFiles(x).ToList().ForEach(y => { File.Copy(y, Path.Combine(tempPath, Path.GetFileName(x), Path.GetFileName(y))); }); });

            var sut = new PackageCacher(_fixture.Settings, _fixture.FileHelper, _fixture.HttpHelper, _fixture.ChocolateyHelper, null);

            // Act
            sut.Purge(tempPath);

            // Assert
            var resultDirs = Directory.EnumerateDirectories(tempPath).ToList();
            resultDirs.Should().HaveCount(5);
            resultDirs.Should().Contain(x => Path.GetFileName(x).Equals("vscode.install.1.69.0", StringComparison.OrdinalIgnoreCase));
            resultDirs.Should().NotContain(x => Path.GetFileName(x).Equals("vscode.install.1.68.1", StringComparison.OrdinalIgnoreCase));

            var result = Directory.EnumerateFiles(tempPath).ToList();
            result.Should().HaveCount(9);
            result.Should().Contain(x => Path.GetFileName(x).Equals("vscode.install.1.69.0.nupkg", StringComparison.OrdinalIgnoreCase));
            result.Should().NotContain(x => Path.GetFileName(x).Equals("vscode.install.1.68.1.nupkg", StringComparison.OrdinalIgnoreCase));
        }

        [Theory]
        [InlineData("nupkg", "download_1.cmd", 3)]
        [InlineData("nupkg", "download_2.cmd", 4)]
        [InlineData("nupkg", "download_3.cmd", 3)]
        [InlineData("blabla", "download_4.cmd", 1)]
        public void GetMissing(string filePattern, string fileName, int expectedCount)
        {
            // Arrange
            var tempPath = _fixture.GetTemp();
            Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Resources", "nupkg")).ToList().Where(x => Path.GetFileName(x).EndsWith(filePattern)).ToList().ForEach(x => { File.Copy(x, Path.Combine(tempPath, Path.GetFileName(x))); });
            var downloads = _fixture.ChocolateyHelper.GetDownloadList(Path.Combine(AppContext.BaseDirectory, "Resources", fileName));

            var sut = new PackageCacher(_fixture.Settings, _fixture.FileHelper, _fixture.HttpHelper, _fixture.ChocolateyHelper, null);

            // Act
            var result = sut.GetMissingFromDownloadsAndDependencies(downloads, tempPath, true);

            // Assert
            result.Should().HaveCount(expectedCount);
        }

        [Theory]
        [InlineData("download_1.cmd", 0)]
        [InlineData("download_2.cmd", 0)]
        [InlineData("download_3.cmd", 0)]
        [InlineData("download_4.cmd", 0)]
        public void GetLastVersions(string fileName, int expectedCount)
        {
            // Arrange
            var downloads = _fixture.ChocolateyHelper.GetDownloadList(Path.Combine(AppContext.BaseDirectory, "Resources", fileName));
            var sut = new PackageCacher(_fixture.Settings, _fixture.FileHelper, _fixture.HttpHelper, _fixture.ChocolateyHelper, null);

            // Act
            var result = sut.GetLastVersions(downloads, false);

            // Assert
            result.Should().HaveCountGreaterThan(expectedCount);
        }

        [Theory]
        [InlineData("vscode.install")]
        [InlineData("rclone")]
        [InlineData("cryptomator")]
        [InlineData("minikube")]
        [InlineData("firefox")]
        public void CachePackage(string id)
        {
            // Arrange
            var tempPath = _fixture.GetTemp();
            var package = _fixture.ChocolateyHelper.GetLastVersion(id);
            var filePath = Path.Combine(tempPath, package.FileName);
            var sut = new PackageCacher(_fixture.Settings, _fixture.FileHelper, _fixture.HttpHelper, _fixture.ChocolateyHelper, null);

            // Act
            var result = sut.CachePackage(package, tempPath);

            // Assert
            File.Exists(filePath).Should().BeTrue();
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("cryptomator")]
        public void CheckInstallScript(string id)
        {
            // Arrange
            var tempPath = _fixture.GetTemp();
            var package = _fixture.ChocolateyHelper.GetLastVersion(id);
            var filePath = Path.Combine(tempPath, package.FileName);
            var sut = new PackageCacher(_fixture.Settings, _fixture.FileHelper, _fixture.HttpHelper, _fixture.ChocolateyHelper, null);

            // Act
            var result = sut.CachePackage(package, tempPath);

            // Assert
            File.Exists(filePath).Should().BeTrue();
            result.Should().BeTrue();
        }
    }
}