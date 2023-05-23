using System.Web;

namespace ChocolateyStoreCoreTestsE2E
{
    [ExcludeFromCodeCoverage]
    public class ChocolateyHelperTests : IClassFixture<TestFixture>
    {
        readonly TestFixture _fixture;

        public ChocolateyHelperTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("azure-cli", "", 1, "msi")]
        [InlineData("docker", "", 1, "exe")]
        [InlineData("kubernetes-helm", "", 1, "zip")]
        [InlineData("postman", "", 1, "exe")]
        [InlineData("vscode.install", "", 2, "exe")]
        [InlineData("cryptomator", "1.6.11", 1, "msi")]
        public void DownloadMetadataForPackageId(string id, string version, int count, string expectedFileType)
        {
            // Arrange
            string repo = @"http://xyz";
            string folder = @"c:\folder";
            string folderName = Path.GetFileName(folder);
            string installScript = Path.Combine(AppContext.BaseDirectory, "Resources", $"chocolateyInstall_{id}.ps1");
            string originalContent = new StreamReader(Path.Combine(AppContext.BaseDirectory, "Resources", installScript)).ReadToEnd();

            // Act
            (string content, var downloads, string fileType) = _fixture.ChocolateyHelper.ExtractAndRewriteUrls(originalContent, folder, repo, id, version);

            // Assert
            content.Should().NotBeNull();
            downloads.Should().HaveCount(count);
            downloads.ForEach(x =>
            {
                string fileName = Path.GetFileName(x.Path);
                content.ToLower().Should().Contain($"{repo.ToLower()}/{folderName.ToLower()}/{HttpUtility.UrlPathEncode(fileName.ToLower())}");
                x.Path.ToLower().Should().EndWith(expectedFileType.ToLower());

                if (!x.Url.ToLower().EndsWith(fileType.ToLower()))
                {
                    (x.Url + "." + fileType).ToLower().Should().EndWith(fileType.ToLower());
                    HttpUtility.UrlDecode(x.Url + "." + fileType).ToLower().Should().EndWith((fileName).ToLower());
                }
                else
                {
                    x.Url.ToLower().Should().EndWith(fileType.ToLower());
                    HttpUtility.UrlDecode(x.Url).ToLower().Should().EndWith(fileName.ToLower());
                }
            });
        }

        [Theory]
        [InlineData("azcopy", 0)]
        [InlineData("vscode", 1)]
        [InlineData("vscode.install", 1)]
        public void GetLastVersion(string id, int countDependencies)
        {
            // Arrange

            // Act
            var result = _fixture.ChocolateyHelper.GetLastVersion(id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Version.OriginalVersion.Should().NotBeEmpty();
            if (countDependencies > 0)
                result.Dependencies.Should().HaveCountGreaterThanOrEqualTo(countDependencies);
        }

        [Theory]
        [InlineData("download_1.cmd", 0)]
        [InlineData("download_2.cmd", 0)]
        [InlineData("download_3.cmd", 0)]
        [InlineData("download_4.cmd", 0)]
        public void FlattenDependenciesSimple(string fileName, int expectedCount)
        {
            // Arrange
            var downloads = _fixture.ChocolateyHelper.GetDownloadList(Path.Combine(AppContext.BaseDirectory, "Resources", fileName));
            var packages = downloads.Select(x => _fixture.ChocolateyHelper.GetLastVersion(x)).Where(x => x != null).ToList();

            // Act
            var result = _fixture.ChocolateyHelper.FlattenDependenciesSimple(packages);

            // Assert
            result.Should().HaveCountGreaterThan(expectedCount);
        }

        [Theory]
        [InlineData("download_1.cmd", 15)]
        [InlineData("download_2.cmd", 16)]
        [InlineData("download_3.cmd", 14)]
        [InlineData("download_4.cmd", 4)]
        public void FlattenDependencies(string fileName, int expectedCount)
        {
            // Arrange
            var downloads = _fixture.ChocolateyHelper.GetDownloadList(Path.Combine(AppContext.BaseDirectory, "Resources", fileName));
            var packages = downloads.Select(x => _fixture.ChocolateyHelper.GetLastVersion(x)).Where(x => x != null).ToList();

            // Act
            var result = _fixture.ChocolateyHelper.FlattenDependencies(packages);

            // Assert
            result.Should().HaveCount(expectedCount);
        }
    }
}