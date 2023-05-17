namespace ChocolateStoreCoreTests
{
    [ExcludeFromCodeCoverage]
    public class PackageCacherTests : IClassFixture<TestFixture>
    {

        public PackageCacherTests(TestFixture fixture)
        {
        }

        [Theory]
        [InlineData(false, 3, 1, 3)]
        [InlineData(true, 3, 2, 2)]
        public void GetLastVersions(bool flatten, int count, int countFlattened, int countResult)
        {
            // Arrange
            var fixture = TestFixture.GetFixture();

            var packages = fixture.CreateMany<ChocolateyPackage>(count).ToList();
            var downloads = fixture.CreateMany<string>(count).ToList();

            var chocolateyHelper = fixture.Freeze<Mock<IChocolateyHelper>>();
            Enumerable.Range(0, count).ToList().ForEach(x => { chocolateyHelper.Setup(_ => _.GetLastVersion(downloads[x])).Returns(packages[x]); });
            chocolateyHelper.Setup(_ => _.FlattenDependencies(It.IsAny<List<ChocolateyPackage>>())).Returns(packages.GetRange(0, countFlattened));

            var sut = fixture.Create<PackageCacher>();

            // Act
            var result = sut.GetLastVersions(downloads, flatten);

            // Assert
            result.Should().HaveCount(countResult);
        }

        [Fact]
        public void PurgeFiles()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();

            var packageFolders = fixture.Create<List<string>>();
            var nupkgs = fixture.Create<List<StorePackage>>();

            var chocolateyHelper = fixture.Freeze<Mock<IChocolateyHelper>>();
            chocolateyHelper.Setup(_ => _.GetPackagesInventory(It.IsAny<string>())).Returns(nupkgs);

            var fileHelper = fixture.Freeze<Mock<IFileHelper>>();
            fileHelper.Setup(_ => _.DirectoryExists(It.IsAny<string>())).Returns(false);
            fileHelper.Setup(_ => _.FileExists(It.IsAny<string>())).Returns(false);
            fileHelper.Setup(_ => _.DirectoryDelete(It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.FileDelete(It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.GetDirectoryNames(It.IsAny<string>())).Returns(packageFolders);

            string path = fixture.Create<string>();
            
            var sut = fixture.Create<PackageCacher>();

            // Act
            var result = sut.Purge(path);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetMissingFromDownloadsAndDependenciesLocalOnly()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();

            var packageFolders = fixture.Create<List<string>>();
            var nupkgs = fixture.Create<List<StorePackage>>();
            var downloads = fixture.Create<List<string>>();

            var chocolateyHelper = fixture.Freeze<Mock<IChocolateyHelper>>();
            chocolateyHelper.Setup(_ => _.GetPackagesInventory(It.IsAny<string>())).Returns(nupkgs);
            chocolateyHelper.Setup(_ => _.GetPackageWithDependenciesFromNupkgFile(It.IsAny<MemoryStream>())).Returns(fixture.Create<ChocolateyPackage>());

            var fileHelper = fixture.Freeze<Mock<IFileHelper>>();
            fileHelper.Setup(_ => _.DirectoryExists(It.IsAny<string>())).Returns(false);
            fileHelper.Setup(_ => _.FileExists(It.IsAny<string>())).Returns(false);
            fileHelper.Setup(_ => _.DirectoryDelete(It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.FileDelete(It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.GetDirectoryNames(It.IsAny<string>())).Returns(packageFolders);

            string path = fixture.Create<string>();

            var sut = fixture.Create<PackageCacher>();

            // Act
            var result = sut.GetMissingFromDownloadsAndDependencies(downloads, path, true);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void CachePackage()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();

            var chocolateyPackage = fixture.Create<ChocolateyPackage>();
            var packageFolders = fixture.Create<List<string>>();
            var nupkgs = fixture.Create<List<StorePackage>>();
            var download1 = fixture.Create<Download>();
            var download2 = fixture.Create<Download>();
            var downloadPath = fixture.Create<string>();
            var downloads = new List<Download>{ download1, download2 };
            var installFileContent = fixture.Create<string>();
            string path = fixture.Create<string>();

            var chocolateyHelper = fixture.Freeze<Mock<IChocolateyHelper>>();
            chocolateyHelper.Setup(_ => _.GetPackagesInventory(It.IsAny<string>())).Returns(nupkgs);
            chocolateyHelper.Setup(_ => _.GetPackageWithDependenciesFromNupkgFile(It.IsAny<MemoryStream>())).Returns(fixture.Create<ChocolateyPackage>());
            chocolateyHelper.Setup(_ => _.ExtractAndRewriteUrls(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(() => ("abd", downloads, "def"));

            var httpHelper = fixture.Freeze<Mock<IHttpHelper>>();
            httpHelper.Setup(_ => _.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(downloadPath);

            var fileHelper = fixture.Freeze<Mock<IFileHelper>>();
            fileHelper.Setup(_ => _.FileExists(Path.Combine(path, chocolateyPackage.FileName))).Returns(false);
            fileHelper.Setup(_ => _.FileExists(download1.Path)).Returns(true);
            fileHelper.Setup(_ => _.FileExists(download2.Path)).Returns(true);
            fileHelper.Setup(_ => _.FileExists(downloadPath)).Returns(true);
            fileHelper.Setup(_ => _.GetContentFromZip(It.IsAny<string>(), It.IsAny<string>())).Returns(installFileContent);
            fileHelper.Setup(_ => _.UpdateContentInZip(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.DirectoryExists(It.IsAny<string>())).Returns(false);
            fileHelper.Setup(_ => _.DirectoryCreateDirectory(It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.GetDirectoryNames(It.IsAny<string>())).Returns(packageFolders);

            var sut = fixture.Create<PackageCacher>();

            // Act
            var result = sut.CachePackage(chocolateyPackage, path);

            // Assert
            result.Should().BeTrue();
        }
    }
}
