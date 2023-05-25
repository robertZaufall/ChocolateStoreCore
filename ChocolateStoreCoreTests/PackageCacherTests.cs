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
        public void PurgeFilesWithPackages()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();

            var packageFolders = fixture.Create<List<string>>();
            var nupkgs = fixture.CreateMany<StorePackage>(2).ToList();
            nupkgs[0].Id = "Test";
            nupkgs[1].Id = "Test";
            nupkgs[0].Version = new NuGet.Versioning.NuGetVersion("1.0.0");
            nupkgs[1].Version = new NuGet.Versioning.NuGetVersion("1.0.1");

            var chocolateyHelper = fixture.Freeze<Mock<IChocolateyHelper>>();
            chocolateyHelper.Setup(_ => _.GetPackagesInventory(It.IsAny<string>())).Returns(nupkgs);

            var fileHelper = fixture.Freeze<Mock<IFileHelper>>();
            fileHelper.Setup(_ => _.DirectoryExists(It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.FileExists(It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.DirectoryDelete(It.IsAny<string>())).Returns(true).Verifiable();
            fileHelper.Setup(_ => _.FileDelete(It.IsAny<string>())).Returns(true).Verifiable();
            fileHelper.Setup(_ => _.GetDirectoryNames(It.IsAny<string>())).Returns(packageFolders);

            string path = fixture.Create<string>();

            var sut = fixture.Create<PackageCacher>();

            // Act
            var result = sut.Purge(path);

            // Assert
            result.Should().BeTrue();
            fileHelper.Verify(_ => _.DirectoryDelete(nupkgs[0].Folder), Times.Exactly(1));
            fileHelper.Verify(_ => _.FileDelete(nupkgs[0].Path), Times.Exactly(1));
            
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


        [Fact]
        public void CachePackage_FileExists_Return_True()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();

            var chocolateyPackage = fixture.Create<ChocolateyPackage>();
            var packageFolders = fixture.Create<List<string>>();
            string path = fixture.Create<string>();

            var chocolateyHelper = fixture.Freeze<Mock<IChocolateyHelper>>();
            var httpHelper = fixture.Freeze<Mock<IHttpHelper>>();

            var fileHelper = fixture.Freeze<Mock<IFileHelper>>();
            fileHelper.Setup(_ => _.FileExists(Path.Combine(path, chocolateyPackage.FileName))).Returns(true);

            var sut = fixture.Create<PackageCacher>();

            // Act
            var result = sut.CachePackage(chocolateyPackage, path);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void CachePackage_WhatIf()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();

            var chocolateyPackage = fixture.Create<ChocolateyPackage>();
            var packageFolders = fixture.Create<List<string>>();
            var nupkgs = fixture.Create<List<StorePackage>>();
            var download1 = fixture.Create<Download>();
            var download2 = fixture.Create<Download>();
            var downloadPath = fixture.Create<string>();
            var downloads = new List<Download> { download1, download2 };
            var installFileContent = fixture.Create<string>();

            string sourcePath = fixture.Create<string>();
            string targetPath = fixture.Create<string>();

            var chocolateyHelper = fixture.Freeze<Mock<IChocolateyHelper>>();
            chocolateyHelper.Setup(_ => _.GetPackagesInventory(It.IsAny<string>())).Returns(nupkgs);
            chocolateyHelper.Setup(_ => _.GetPackageWithDependenciesFromNupkgFile(It.IsAny<MemoryStream>())).Returns(fixture.Create<ChocolateyPackage>());
            chocolateyHelper.Setup(_ => _.ExtractAndRewriteUrls(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(() => ("abd", downloads, "def"));

            var httpHelper = fixture.Freeze<Mock<IHttpHelper>>();

            var fileHelper = fixture.Freeze<Mock<IFileHelper>>();
            fileHelper.Setup(_ => _.FileCopy(It.IsAny<string>(), It.IsAny<string>())).Returns(true).Verifiable();

            fileHelper.Setup(_ => _.FileExists(Path.Combine(sourcePath, chocolateyPackage.FileName))).Returns(false);
            fileHelper.Setup(_ => _.FileExists(download1.Path)).Returns(true);
            fileHelper.Setup(_ => _.FileExists(download2.Path)).Returns(true);
            fileHelper.Setup(_ => _.FileExists(downloadPath)).Returns(true);
            fileHelper.Setup(_ => _.GetContentFromZip(It.IsAny<string>(), It.IsAny<string>())).Returns(installFileContent);
            fileHelper.Setup(_ => _.UpdateContentInZip(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.DirectoryExists(It.IsAny<string>())).Returns(false);
            fileHelper.Setup(_ => _.DirectoryCreateDirectory(It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.GetDirectoryNames(It.IsAny<string>())).Returns(packageFolders);

            fileHelper.Setup(_ => _.WriteDummyFile(It.IsAny<string>())).Returns(true).Verifiable();

            var sut = fixture.Create<PackageCacher>();

            // Act
            var result = sut.CachePackage(chocolateyPackage, sourcePath, targetPath, true);

            // Assert
            result.Should().BeTrue();
            fileHelper.Verify(_ => _.FileCopy(Path.Combine(sourcePath, chocolateyPackage.FileName), Path.Combine(targetPath, chocolateyPackage.FileName)), Times.Exactly(1));
            fileHelper.Verify(_ => _.WriteDummyFile(It.IsAny<string>()), Times.Exactly(downloads.Count));
        }
    }
}
