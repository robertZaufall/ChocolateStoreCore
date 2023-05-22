using Moq.Protected;

namespace ChocolateStoreCoreTestsIntegration
{
    [ExcludeFromCodeCoverage]
    public class HttpHelperTestsIntegration : IClassFixture<TestFixtureIntegration>
    {
        readonly TestFixtureIntegration _fixture;

        public HttpHelperTestsIntegration(TestFixtureIntegration fixture)
        {
            _fixture = fixture;
        }

        [Theory(Skip = "no http")]
        [InlineData("azcopy", 0)]
        [InlineData("vscode", 1)]
        public void GetMetadataForPackageId(string id, int countDependencies)
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(_fixture.DummyResponse())
                .Verifiable();

            var chocolateyHelper = _fixture.GetChocolateyHelper(handlerMock);
            var httpHelper = _fixture.GetHttpHelper(handlerMock);

            // Act
            var content = httpHelper.GetMetadataForPackageId(id);
            var result = chocolateyHelper.ParseMetadata(id, content);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Version.OriginalVersion.Should().NotBeEmpty();
            if (countDependencies > 0)
                result.Dependencies.Should().HaveCountGreaterThanOrEqualTo(countDependencies);
        }

        [Fact(Skip = "no http")]
        public void DownloadFile()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(_fixture.DummyResponse())
                .Verifiable();

            var chocolateyHelper = _fixture.GetChocolateyHelper(handlerMock);
            var httpHelper = _fixture.GetHttpHelper(handlerMock);

            var tempPath = _fixture.GetTemp();
            var package = chocolateyHelper.GetLastVersion("vscode");
            var filePath = Path.Combine(tempPath, package.FileName);

            // Act
            var result = httpHelper.DownloadFile(package.DownloadUrl, filePath);

            // Assert
            result.Should().NotBeNull();
            File.Exists(filePath).Should().BeTrue();
        }
    }
}
