using ChocolateStoreCore.Helpers;

namespace ChocolateStoreCoreTests
{
    [ExcludeFromCodeCoverage]
    public class AppTests
    {
        private readonly ISettings _settings;

        public AppTests()
        {
            var settings = new Mock<ISettings>();
            settings.Setup(_ => _.FolderPath).Returns(Path.Combine(Path.GetTempPath(), "test1.log"));
            settings.Setup(_ => _.DownloadListPath).Returns(Path.Combine(Path.GetTempPath(), "test2.log"));
            _settings = settings.Object;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async void RunTest(bool whatif)
        {
            // Arrange
            var fixture = TestFixture.GetFixture();
            
            var chocolateyHelper = fixture.Freeze<Mock<IChocolateyHelper>>();
            chocolateyHelper.Setup(_ => _.GetDownloadList(It.IsAny<string>())).Returns(new List<string> { "a", "b" });
            chocolateyHelper.Setup(_ => _.FlattenDependencies(It.IsAny<List<ChocolateyPackage>>())).Returns(new List<ChocolateyPackage> { new ChocolateyPackage() }).Verifiable();

            var packageCacher = fixture.Freeze<Mock<IPackageCacher>>();
            packageCacher.Setup(_ => _.Purge(It.IsAny<string>(), It.IsAny<bool>())).Returns(true);
            packageCacher.Setup(_ => _.GetLastVersions(It.IsAny<List<string>>(), It.IsAny<bool>())).Returns(new List<ChocolateyPackage> { new ChocolateyPackage() }).Verifiable();

            var fileHelper = fixture.Freeze<Mock<IFileHelper>>();
            fileHelper.Setup(_ => _.DirectoryCreateDirectory(It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.DirectoryExists(It.IsAny<string>())).Returns(true);

            var sut = fixture.Create<App>();

            // Act
            var result = await sut.Run(whatif);
            
            // Assert
            result.Should().Be(0);
        }
    }
}
