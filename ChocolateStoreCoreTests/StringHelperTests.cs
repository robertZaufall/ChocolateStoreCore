using System.IO;
using System.Text.RegularExpressions;

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
            var downloads = StringHelper.GetOriginalUrls(originalUrl, id, version, "###");

            downloads.Should().NotBeNull();
            downloads.Should().HaveCount(1);
            var download = downloads[0];
            download.Should().Be(urlExpected);
        }

        [Fact]
        public void ReplaceTokensByVariables_Gradle()
        {
            string input =
@"$packageName = 'gradle'
$version = '8.3'
$checksum = 'BB09982FDF52718E4C7B25023D10DF6D35A5FFF969860BDF5A5BD27A3AB27A9E'
$url = ""https://services.gradle.org/distributions/gradle-$version-all.zip""
$installDir = Split-Path -parent $MyInvocation.MyCommand.Definition

Install-ChocolateyZipPackage $packageName $url $installDir -Checksum $checksum -ChecksumType 'sha256'

$gradle_home = Join-Path $installDir ""$packageName-$version""
$gradle_bat = Join-Path $gradle_home 'bin/gradle.bat'

Install-ChocolateyEnvironmentVariable ""GRADLE_HOME"" $gradle_home 'Machine'
Install-BinFile -Name 'gradle' -Path $gradle_bat
";

            string expected =
@"$packageName = 'gradle'
$version = '8.3'
$checksum = 'BB09982FDF52718E4C7B25023D10DF6D35A5FFF969860BDF5A5BD27A3AB27A9E'
$url = ""https://services.gradle.org/distributions/gradle-8.3-all.zip""
$installDir = Split-Path -parent $MyInvocation.MyCommand.Definition
Install-ChocolateyZipPackage $packageName $url $installDir -Checksum $checksum -ChecksumType 'sha256'
$gradle_home = Join-Path $installDir ""$packageName-$version""
$gradle_bat = Join-Path $gradle_home 'bin/gradle.bat'
Install-ChocolateyEnvironmentVariable ""GRADLE_HOME"" $gradle_home 'Machine'
Install-BinFile -Name 'gradle' -Path $gradle_bat
";

            var result = StringHelper.ReplaceTokensByVariables(input);

            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Contain("$url = \"https://services.gradle.org/distributions/gradle-8.3-all.zip\"");
            result.Replace("\r\n", "\n").Should().Be(expected.Replace("\r\n", "\n"));
        }

        [Fact]
        public void ReplaceTokensByVariables_Firefox()
        {
            var input = TestFixture.ReadFile("chocolateyInstall_firefox.ps1");

            var result = StringHelper.ReplaceTokensByVariables(input);

            result.Should().NotBeNullOrWhiteSpace();
            Regex.Replace(result, @"\s+", "").Should().Be(Regex.Replace(input, @"\s+", ""));
        }

        [Fact]
        public void ReplaceTokensByVariables_Pnpm()
        {
            var input =
@"$packageName = $Env:chocolateyPackageName
$packagePnpmVersion = [version]([regex]'^\d+(\.\d+){2}').Match($Env:chocolateyPackageVersion).Value
$pnpmExecutableUrl = ""https://github.com/pnpm/pnpm/releases/download/v$packagePnpmVersion/pnpm-win-x64.exe""
";

            var result = StringHelper.ReplaceTokensByVariables(input, "pnpm", "10.33.0");

            result.Should().Contain("$pnpmExecutableUrl = \"https://github.com/pnpm/pnpm/releases/download/v10.33.0/pnpm-win-x64.exe\"");
        }

        [Theory]
        [InlineData("a", "a")]
        [InlineData("a ", "a")]
        [InlineData("a b", "a")]
        [InlineData("a b b", "a")]
        [InlineData("", "")]
        [InlineData("abcdefghijk", "abcdefghijk")]
        [InlineData("test 1.0.0", "test")]
        [InlineData("nodejs-lts:18.16.1", "nodejs-lts")]
        public void GetPackageId_From_PackageId(string packageId, string id)
        {
            var resultId = StringHelper.GetPackageIdFromString(packageId);

            resultId.Should().NotBeNull();
            resultId.Should().Be(id);
        }

        [Theory]
        [InlineData("a", "")]
        [InlineData("a ", "")]
        [InlineData("a b", "b")]
        [InlineData("a b b", "b b")]
        [InlineData("", "")]
        [InlineData("test 1.0.0", "1.0.0")]
        [InlineData("nodejs-lts:18.16.1", "18.16.1")]
        public void GetVersion_From_PackageId(string packageId, string id)
        {
            var resultId = StringHelper.GetVersionFromString(packageId);

            resultId.Should().NotBeNull();
            resultId.Should().Be(id);
        }
    }
}


