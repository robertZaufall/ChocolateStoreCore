using NuGet.Packaging;
using NuGet.Versioning;
using System.Text;

namespace ChocolateStoreCoreTests
{
    [ExcludeFromCodeCoverage]
    public class ChocolateyHelperTests : IClassFixture<TestFixture>
    {
        readonly TestFixture _fixture;
        private readonly Mock<ISettings> _settingsMock = new Mock<ISettings>();

        public ChocolateyHelperTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer.exe", "exe", "Docker Desktop Installer.exe", "bla", "123", @"http://xyz", 1)]
        [InlineData("https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer", "EXE", "Docker Desktop Installer.exe", "bla", "123", @"http://xyz", 1)]
        [InlineData("https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer", "", "Docker Desktop Installer", "bla", "123", @"http://xyz", 1)]
        [InlineData("https://desktop.docker.com/win/main/amd64/82475/Docker%20Desktop%20Installer", "", "Docker Desktop Installer", "", "", @"http://xyz", 1)]
        [InlineData("if (\"https://static.rust-lang.org/dist/2023-01-10/rust-mingw-1.66.1-i686-pc-windows-gnu.tar.gz\" -ne \"\") {", "gz", "rust-mingw-1.66.1-i686-pc-windows-gnu.tar.gz", "", "", @"http://xyz", 1)]
        [InlineData("https://123.xyz/test.zip", "zip", "test.zip", "bla", "123", @"https://123.xyz", 0)]
        public void ExtractAndRewriteUrls(string originalUrl, string originalFileType, string fileNameExpected, string id, string version, string repo, int cntDownloads)
        {
            // Arrange
            var fixture = TestFixture.GetFixture();

            string folder = @"c:\folder";
            string folderName = Path.GetFileName(folder);
            var originalContent = new StringBuilder();
            originalContent.AppendLine($"$url = '{originalUrl}'");
            originalContent.AppendLine($"fileType = '{originalFileType}'");

            var httpHelper = fixture.Freeze<Mock<IHttpHelper>>();
            httpHelper.Setup(_ => _.CheckUrl(It.IsAny<string>())).Returns((string s) => s);
            
            var sut = fixture.Create<ChocolateyHelper>();

            // Act
            (string content, var downloads, string fileType) = sut.ExtractAndRewriteUrls(originalContent.ToString(), folder, repo, id, version);

            // Assert
            content.Should().NotBeNull();
            content.ToLower().Should().Contain($"{repo.ToLower()}");
            downloads.Should().HaveCount(cntDownloads);
            if (cntDownloads > 0)
            {
                var download = downloads[0];
                string fileName = Path.GetFileName(download.Path);
                fileName.Should().Be(fileNameExpected);
                content.ToLower().Should().Contain($"{repo.ToLower()}/{folderName.ToLower()}/{System.Web.HttpUtility.UrlPathEncode(fileName.ToLower())}");
                download.Path.ToLower().Should().Be($"{folder.ToLower()}\\{fileName.ToLower()}");
            }
            else
            {
                originalUrl.ToLower().Should().Contain($"{repo.ToLower()}");
                content.ToLower().Should().Contain($"{originalUrl.ToLower()}");
            }
        }

        [Theory]
        [InlineData("ChocolateyGUI", "1.0.0", "https://community.chocolatey.org/api/v2/package/ChocolateyGUI/1.0.0", 3)]
        public void ParseMetadata(string originalId, string version, string downloadUrl, int countDependencies)
        {
            // Arrange
            var fixture = TestFixture.GetFixture();
            string content = TestFixture.GetExampleMetadata();

            var sut = fixture.Create<ChocolateyHelper>();

            // Act
            var chocolateyPackage = sut.ParseMetadata(originalId, content);

            // Assert
            chocolateyPackage.Should().NotBeNull();
            chocolateyPackage.Id.ToLower().Should().Be(originalId.ToLower());
            chocolateyPackage.Version.OriginalVersion.ToLower().Should().Be(version.ToLower());
            chocolateyPackage.DownloadUrl.ToLower().Should().Be(downloadUrl.ToLower());
            chocolateyPackage.Dependencies.Should().HaveCount(countDependencies);
        }

        [Fact]
        public void ParseNupkg()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();
            var stream = TestFixture.GetNupkgWithDependencies();
            var sut = fixture.Create<ChocolateyHelper>();

            // Act
            var storePackage = sut.ParseNupkg(stream);

            // Assert
            storePackage.Should().NotBeNull();
            storePackage.Id.Should().Be("test");
            storePackage.Version.OriginalVersion.Should().Be("1.0.0");
        }

        [Fact]
        public void GetPackageWithDependenciesFromNupkgFile()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();
            var stream = TestFixture.GetNupkgWithDependencies();

            var fileHelper = fixture.Freeze<Mock<IFileHelper>>();
            fileHelper.Setup(_ => _.GetStream(It.IsAny<string>())).Returns(stream);

            var sut = fixture.Create<ChocolateyHelper>();

            // Act
            var chocolateyPackage = sut.GetPackageWithDependenciesFromNupkgFile(stream);

            // Assert
            chocolateyPackage.Should().NotBeNull();
            chocolateyPackage.Id.Should().Be("test");
            chocolateyPackage.Version.OriginalVersion.Should().Be("1.0.0");
            chocolateyPackage.Dependencies.Should().HaveCount(2);
        }

        [Fact]
        public void GetPackageInventory()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();
            var stream = TestFixture.GetNupkgWithDependencies();
            var path = fixture.Create<string>();

            var files = new List<string> { fixture.Create<string>() };
            var fileHelper = fixture.Freeze<Mock<IFileHelper>>();
            fileHelper.Setup(_ => _.GetNupkgFiles(It.IsAny<string>())).Returns(files);
            fileHelper.Setup(_ => _.GetStream(It.IsAny<string>())).Returns(stream);
            fileHelper.Setup(_ => _.DirectoryExists(It.IsAny<string>())).Returns(true);

            var sut = fixture.Create<ChocolateyHelper>();

            // Act
            var storePackages = sut.GetPackagesInventory(path);

            // Assert
            storePackages.Should().HaveCount(1);
        }

        [Fact]
        public void GetPackageDependenciesFromNuspec()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();
            var sut = fixture.Create<ChocolateyHelper>();
            var dependencies = new List<Dependency>();

            // Act
            using (var nugetPackage = new PackageArchiveReader(TestFixture.GetNupkgWithDependencies()))
            {
                dependencies = sut.GetPackageDependenciesFromNuspec(nugetPackage.NuspecReader);
            }

            // Assert
            dependencies.Should().HaveCount(2);
        }

        [Fact]
        public void GetDownloadListSimple()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();
            string path = "c:\\folder";

            var fileHelper = fixture.Freeze<Mock<IFileHelper>>();
            fileHelper.Setup(_ => _.FileExists(It.IsAny<string>())).Returns(true);
            fileHelper.Setup(_ => _.GetListFromStream(It.IsAny<string>())).Returns(new List<string>
            {
                "a",
                @"chocolatestore.exe https://chocolatey.org/api/v2/package/b",
                @"rem chocolatestore.exe https://chocolatey.org/api/v2/package/c",
                "rem d"
            });

            var sut = fixture.Create<ChocolateyHelper>();

            // Act
            var result = sut.GetDownloadList(path);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(new List<string> { "a", "b" });
        }

        [Fact]
        public void GetLastVersion()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();
            string content = TestFixture.GetExampleMetadata();

            var id = "ChocolateyGUI";

            var httpHelper = fixture.Freeze<Mock<IHttpHelper>>();
            httpHelper.Setup(_ => _.GetMetadataForPackageId(It.IsAny<string>())).Returns(content);

            var sut = fixture.Create<ChocolateyHelper>();

            // Act
            var result = sut.GetLastVersion(id);

            // Assert
            result.Should().NotBeNull();
            result.Id.ToLower().Should().Be(id.ToLower());
        }

        [Fact]
        public void FlattenDependenciesSimple()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();
            string content = TestFixture.GetExampleMetadata();
            var dependencies = new List<Dependency>
            {
                new Dependency { Id = "a", Version = new NuGetVersion("1.0.0") },
                new Dependency { Id = "b", Version = new NuGetVersion("1.0.0") },
                new Dependency { Id = "c", Version = new NuGetVersion("1.0.0") }
            };
            var chocolateyPackages = new List<ChocolateyPackage>
            {
                new ChocolateyPackage
                {
                    Id = "d",
                    Version = new NuGetVersion("1.0.0"),
                    Dependencies = dependencies
                }
            };

            var httpHelper = fixture.Freeze<Mock<IHttpHelper>>();
            httpHelper.Setup(_ => _.GetMetadataForPackageId(It.IsAny<string>())).Returns(content);

            var sut = fixture.Create<ChocolateyHelper>();

            // Act
            var result = sut.FlattenDependenciesSimple(chocolateyPackages);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThanOrEqualTo(chocolateyPackages.Count);
            result.Should().HaveCount(2);
        }

        [Fact]
        public void FlattenDependenciesSimpleWithNullVersion()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();
            string content = TestFixture.GetExampleMetadata();
            var dependencies = new List<Dependency>
            {
                new Dependency { Id = "a", Version = null },
            };
            var chocolateyPackages = new List<ChocolateyPackage>
            {
                new ChocolateyPackage
                {
                    Id = "d",
                    Version = new NuGetVersion("1.0.0"),
                    Dependencies = dependencies
                }
            };

            var httpHelper = fixture.Freeze<Mock<IHttpHelper>>();
            httpHelper.Setup(_ => _.GetMetadataForPackageId(It.IsAny<string>())).Returns(content);

            var sut = fixture.Create<ChocolateyHelper>();

            // Act
            var result = sut.FlattenDependenciesSimple(chocolateyPackages);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThanOrEqualTo(chocolateyPackages.Count);
            result.Should().HaveCount(2);
        }

        [Fact]
        public void FlattenDependencies()
        {
            // Arrange
            var fixture = TestFixture.GetFixture();
            string content = TestFixture.GetExampleMetadata();

            var dependencies = new List<Dependency>
            {
                new Dependency { Id = "a", Version = new NuGetVersion("1.0.0") },
                new Dependency { Id = "b", Version = new NuGetVersion("1.0.0") },
                new Dependency { Id = "c", Version = new NuGetVersion("1.0.0") }
            };
            var chocolateyPackages = new List<ChocolateyPackage>
            {
                new ChocolateyPackage
                {
                    Id = "d",
                    Version = new NuGetVersion("1.0.0"),
                    Dependencies = dependencies
                }
            };

            var httpHelper = fixture.Freeze<Mock<IHttpHelper>>();
            httpHelper.Setup(_ => _.GetMetadataForPackageId(It.IsAny<string>())).Returns(content);

            var sut = fixture.Create<ChocolateyHelper>();

            // Act
            var result = sut.FlattenDependencies(chocolateyPackages);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThanOrEqualTo(chocolateyPackages.Count);
            result.Should().HaveCount(2);
        }
    }
}
