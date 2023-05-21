using System.Web;

namespace ChocolateStoreCoreTestsIntegration
{
    [ExcludeFromCodeCoverage]
    public class ChocolateyHelperTestsIntegration : IClassFixture<TestFixtureIntegration>
    {
        readonly TestFixtureIntegration _fixture;

        public ChocolateyHelperTestsIntegration(TestFixtureIntegration fixture)
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
            string installScript = Path.Combine(AppContext.BaseDirectory, _fixture.ResourceDirName, $"chocolateyInstall_{id}.ps1");
            string originalContent = new StreamReader(Path.Combine(AppContext.BaseDirectory, _fixture.ResourceDirName, installScript)).ReadToEnd();

            var chocolateyHelper = _fixture.GetChocolateyHelper(_fixture.DummyResponse());

            // Act
            (string content, var downloads, string fileType) = chocolateyHelper.ExtractAndRewriteUrls(originalContent, folder, repo, id, version);

            // Assert
            content.Should().NotBeNull();
            downloads.Should().HaveCount(count);
            downloads.ForEach(x =>
            {
                string fileName = Path.GetFileName(x.Path);
                content.ToLower().Should().Contain($"{repo.ToLower()}/{folderName.ToLower()}/{fileName.ToLower()}");
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
        [InlineData("download_1.cmd", "autohotkey;azcopy;azure-cli;docker-desktop;rclone;vscode")]
        [InlineData("download_1.txt", "autohotkey;azcopy;azure-cli;docker-desktop;rclone;vscode")]
        [InlineData("download_6.cmd", "wget")]
        public void GetDownloadList(string fileName, string comparisonList)
        {
            // Arrange
            var tempPath = _fixture.GetTemp();
            var downloadList = Path.Combine(tempPath, fileName);
            File.Copy(Path.Combine(AppContext.BaseDirectory, _fixture.ResourceDirName, fileName), downloadList);

            var chocolateyHelper = _fixture.GetChocolateyHelper(_fixture.DummyResponse());

            // Act
            var files = chocolateyHelper.GetDownloadList(downloadList);

            // Assert
            files.Should().BeEquivalentTo(comparisonList.Split(';').ToList());
        }

        [Theory(Skip = "no http")]
        [InlineData("azcopy", 0)]
        [InlineData("vscode", 1)]
        [InlineData("vscode.install", 1)]
        public void GetLastVersion(string id, int countDependencies)
        {
            // Arrange
            var chocolateyHelper = _fixture.GetChocolateyHelper(_fixture.DummyResponse());

            // Act
            var result = chocolateyHelper.GetLastVersion(id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Version.OriginalVersion.Should().NotBeEmpty();
            if (countDependencies > 0)
                result.Dependencies.Should().HaveCountGreaterThanOrEqualTo(countDependencies);
        }
        
        [Theory(Skip = "no http")]
        [InlineData("download_1.cmd", 0)]
        [InlineData("download_2.cmd", 0)]
        [InlineData("download_3.cmd", 0)]
        [InlineData("download_4.cmd", 0)]
        public void FlattenDependenciesSimple(string fileName, int expectedCount)
        {
            // Arrange
            var chocolateyHelper = _fixture.GetChocolateyHelper(_fixture.DummyResponse());

            var downloads = chocolateyHelper.GetDownloadList(Path.Combine(AppContext.BaseDirectory, _fixture.ResourceDirName, fileName));
            var packages = downloads.Select(x => chocolateyHelper.GetLastVersion(x)).Where(x => x != null).ToList();
            
            // Act
            var result = chocolateyHelper.FlattenDependenciesSimple(packages);

            // Assert
            result.Should().HaveCountGreaterThan(expectedCount);
        }

        [Theory(Skip = "no http")]
        [InlineData("download_1.cmd", 15)]
        [InlineData("download_2.cmd", 16)]
        [InlineData("download_3.cmd", 14)]
        [InlineData("download_4.cmd", 4)]
        public void FlattenDependencies(string fileName, int expectedCount)
        {
            // Arrange
            var chocolateyHelper = _fixture.GetChocolateyHelper(_fixture.DummyResponse());

            var downloads = chocolateyHelper.GetDownloadList(Path.Combine(AppContext.BaseDirectory, _fixture.ResourceDirName, fileName));
            var packages = downloads.Select(x => chocolateyHelper.GetLastVersion(x)).Where(x => x != null).ToList();

            // Act
            var result = chocolateyHelper.FlattenDependencies(packages);

            // Assert
            result.Should().HaveCount(expectedCount);
        }
    }
}
