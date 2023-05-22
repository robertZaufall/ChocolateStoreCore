namespace ChocolateyStoreCoreTestsE2E
{
    [ExcludeFromCodeCoverage]
    public class HttpHelperTests : IClassFixture<TestFixture>
    {
        readonly TestFixture _fixture;

        public HttpHelperTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("azcopy", 0)]
        [InlineData("vscode", 1)]
        [InlineData("vscode.install", 1)]
        public void GetMetadataForPackageId(string id, int countDependencies)
        {
            // Arrange

            // Act
            var content = _fixture.HttpHelper.GetMetadataForPackageId(id);
            var result = _fixture.ChocolateyHelper.ParseMetadata(id, content);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Version.OriginalVersion.Should().NotBeEmpty();
            if (countDependencies > 0)
                result.Dependencies.Should().HaveCountGreaterThanOrEqualTo(countDependencies);
        }

        [Fact]
        public void DownloadFile()
        {
            // Arrange
            var tempPath = _fixture.GetTemp();
            var package = _fixture.ChocolateyHelper.GetLastVersion("vscode");
            var filePath = Path.Combine(tempPath, package.FileName);

            // Act
            var result = _fixture.HttpHelper.DownloadFile(package.DownloadUrl, filePath);

            // Assert
            result.Should().NotBeNull();
            File.Exists(filePath).Should().BeTrue();
        }
    }
}