using Microsoft.Extensions.DependencyInjection;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System.Text;

namespace ChocolateStoreCoreTests
{
    [ExcludeFromCodeCoverage]
    public class TestFixture
    {
        public TestFixture()
        {
        }

        public static IFixture GetFixture()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            fixture.Customizations.Add(new ChocolateStoreSpecimenBuilder());
            return fixture;
        }

        public IHttpClientFactory GetHttpClientFactoryMock(Mock<HttpMessageHandler> handlerMock)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient("chocolatey").ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object);
            return services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        }

        public static Stream GetNupkg()
        {
            PackageBuilder builder = new PackageBuilder();

            var path = Path.Combine(AppContext.BaseDirectory, "resources");

            builder.PopulateFiles(path, new[] { new ManifestFile { Source = "chocolateyInstall.ps1", Target = "tools" } });
            builder.Populate(new ManifestMetadata()
            {
                Id = "test",
                Version = new NuGet.Versioning.NuGetVersion("1.0.0"),
                Title = "test title",
                Authors = new List<string> { "test author" },
                Description = "test description",
            });
            MemoryStream stream = new MemoryStream();
            builder.Save(stream);
            return stream;
        }

        public static MemoryStream GetNupkgWithDependencies()
        {
            PackageBuilder builder = new PackageBuilder();

            var path = Path.Combine(AppContext.BaseDirectory, "resources");

            builder.PopulateFiles(path, new[] { new ManifestFile { Source = "chocolateyInstall.ps1", Target = "tools" } });
            builder.Populate(new ManifestMetadata()
            {
                Id = "test",
                Version = new NuGet.Versioning.NuGetVersion("1.0.0"),
                Title = "test title",
                Authors = new List<string> { "test author" },
                Description = "test description",
                DependencyGroups = new List<PackageDependencyGroup>
                {
                    new PackageDependencyGroup(new NuGetFramework(FrameworkConstants.SpecialIdentifiers.Any) , new List<PackageDependency>
                    {
                        new PackageDependency("test1", new VersionRange(new NuGetVersion("1.0.1"))),
                        new PackageDependency("test2", new VersionRange(new NuGetVersion("1.0.2"))),
                    })
                }
            });
            MemoryStream stream = new MemoryStream();
            builder.Save(stream);
            return stream;
        }

        public static string GetExampleMetadata()
        {
            return "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
                   "<feed xml:base=\"http://community.chocolatey.org/api/v2/\" xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\" xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" xmlns=\"http://www.w3.org/2005/Atom\">" +
                   "  <title type=\"text\">Packages</title>" +
                   "  <id>http://community.chocolatey.org/api/v2/Packages</id>" +
                   "  <updated>2022-07-10T10:32:06Z</updated>" +
                   "  <link rel=\"self\" title=\"Packages\" href=\"Packages\" />" +
                   "  <entry>" +
                   "    <id>http://community.chocolatey.org/api/v2/Packages(Id='ChocolateyGUI',Version='1.0.0')</id>" +
                   "    <title type=\"text\">ChocolateyGUI</title>" +
                   "    <summary type=\"text\">A GUI for Chocolatey</summary>" +
                   "    <updated>2022-07-10T10:30:00Z</updated>" +
                   "    <author>" +
                   "      <name>Chocolatey</name>" +
                   "    </author>" +
                   "    <link rel=\"edit-media\" title=\"V2FeedPackage\" href=\"Packages(Id='ChocolateyGUI',Version='1.0.0')/$value\" />" +
                   "    <link rel=\"edit\" title=\"V2FeedPackage\" href=\"Packages(Id='ChocolateyGUI',Version='1.0.0')\" />" +
                   "    <category term=\"CCR.Website.V2FeedPackage\" scheme=\"http://schemas.microsoft.com/ado/2007/08/dataservices/scheme\" />" +
                   "    <content type=\"application/zip\" src=\"https://community.chocolatey.org/api/v2/package/ChocolateyGUI/1.0.0\" />" +
                   "    <m:properties xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\" xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\">" +
                   "      <d:Version>1.0.0</d:Version>" +
                   "      <d:Title>Chocolatey GUI</d:Title>" +
                   "      <d:Description>Chocolatey GUI is a delicious GUI on top of the Chocolatey command line tool. (...)</d:Description>" +
                   "      <d:Tags xml:space=\"preserve\"> chocolateygui chocolatey admin foss </d:Tags>" +
                   "      <d:Copyright m:null=\"true\"></d:Copyright>" +
                   "      <d:Created m:type=\"Edm.DateTime\">2022-03-21T10:28:24.523</d:Created>" +
                   "      <d:Dependencies>Chocolatey:[1.0.0, 2.0.0):|chocolatey-core.extension:1.3.3:|dotnetfx:4.8.0.20190930:</d:Dependencies>" +
                   "      <d:DownloadCount m:type=\"Edm.Int32\">1706795</d:DownloadCount>" +
                   "      <d:VersionDownloadCount m:type=\"Edm.Int32\">240859</d:VersionDownloadCount>" +
                   "      <d:GalleryDetailsUrl>https://community.chocolatey.org/packages/ChocolateyGUI/1.0.0</d:GalleryDetailsUrl>" +
                   "      <d:ReportAbuseUrl>https://community.chocolatey.org/package/ReportAbuse/ChocolateyGUI/1.0.0</d:ReportAbuseUrl>" +
                   "      <d:IconUrl>https://chocolatey.org/assets/images/nupkg/chocolateyicon.png</d:IconUrl>" +
                   "      <d:IsLatestVersion m:type=\"Edm.Boolean\">true</d:IsLatestVersion>" +
                   "      <d:IsAbsoluteLatestVersion m:type=\"Edm.Boolean\">true</d:IsAbsoluteLatestVersion>" +
                   "      <d:IsPrerelease m:type=\"Edm.Boolean\">false</d:IsPrerelease>" +
                   "      <d:Language m:null=\"true\"></d:Language>" +
                   "      <d:Published m:type=\"Edm.DateTime\">2022-03-21T10:28:24.523</d:Published>" +
                   "      <d:LicenseUrl>https://raw.githubusercontent.com/chocolatey/ChocolateyGUI/develop/LICENSE.txt</d:LicenseUrl>" +
                   "      <d:RequireLicenseAcceptance m:type=\"Edm.Boolean\">false</d:RequireLicenseAcceptance>" +
                   "      <d:PackageHash>rauWbbI4kVy4qfO0v2lSOjyXZikN+iBB/arJMB2sv2J3o4bbYyN23ksqILjUSg2YQsgYCAiyvBwV2kKs8YYtjQ==</d:PackageHash>" +
                   "      <d:PackageHashAlgorithm>SHA512</d:PackageHashAlgorithm>" +
                   "      <d:PackageSize m:type=\"Edm.Int64\">14075834</d:PackageSize>" +
                   "      <d:ProjectUrl>https://github.com/chocolatey/ChocolateyGUI</d:ProjectUrl>" +
                   "      <d:ReleaseNotes>All release notes for Chocolatey GUI can be found on the docs site - https://docs.chocolatey.org/en-us/chocolatey-gui/release-notes</d:ReleaseNotes>" +
                   "      <d:ProjectSourceUrl>https://github.com/chocolatey/ChocolateyGUI</d:ProjectSourceUrl>" +
                   "      <d:PackageSourceUrl>https://github.com/chocolatey/ChocolateyGUI/tree/develop/nuspec/chocolatey</d:PackageSourceUrl>" +
                   "      <d:DocsUrl></d:DocsUrl>" +
                   "      <d:MailingListUrl></d:MailingListUrl>" +
                   "      <d:BugTrackerUrl>https://github.com/chocolatey/ChocolateyGUI/issues</d:BugTrackerUrl>" +
                   "      <d:IsApproved m:type=\"Edm.Boolean\">true</d:IsApproved>" +
                   "      <d:PackageStatus>Approved</d:PackageStatus>" +
                   "      <d:PackageSubmittedStatus>Ready</d:PackageSubmittedStatus>" +
                   "      <d:PackageTestResultUrl></d:PackageTestResultUrl>" +
                   "      <d:PackageTestResultStatus>Exempted</d:PackageTestResultStatus>" +
                   "      <d:PackageTestResultStatusDate m:type=\"Edm.DateTime\" m:null=\"true\"></d:PackageTestResultStatusDate>" +
                   "      <d:PackageValidationResultStatus>Passing</d:PackageValidationResultStatus>" +
                   "      <d:PackageValidationResultDate m:type=\"Edm.DateTime\">2022-03-21T11:03:26.27</d:PackageValidationResultDate>" +
                   "      <d:PackageCleanupResultDate m:type=\"Edm.DateTime\" m:null=\"true\"></d:PackageCleanupResultDate>" +
                   "      <d:PackageReviewedDate m:type=\"Edm.DateTime\">2022-03-21T12:14:08.947</d:PackageReviewedDate>" +
                   "      <d:PackageApprovedDate m:type=\"Edm.DateTime\">2022-03-21T12:14:08.947</d:PackageApprovedDate>" +
                   "      <d:PackageReviewer m:null=\"true\"></d:PackageReviewer>" +
                   "      <d:IsDownloadCacheAvailable m:type=\"Edm.Boolean\">false</d:IsDownloadCacheAvailable>" +
                   "      <d:DownloadCacheStatus>Checked</d:DownloadCacheStatus>" +
                   "      <d:DownloadCacheDate m:type=\"Edm.DateTime\">2022-03-21T13:17:38.407</d:DownloadCacheDate>" +
                   "      <d:DownloadCache m:null=\"true\"></d:DownloadCache>" +
                   "      <d:PackageScanStatus>NotFlagged</d:PackageScanStatus>" +
                   "      <d:PackageScanResultDate m:type=\"Edm.DateTime\">2022-03-21T12:14:08.947</d:PackageScanResultDate>" +
                   "      <d:PackageScanFlagResult>None</d:PackageScanFlagResult>" +
                   "    </m:properties>" +
                   "  </entry>" +
                   "</feed>";
        }

        public static string GetExampleInstallScript1()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("$ErrorActionPreference = 'Stop';");
            sb.AppendLine("$packageArgs = @{");
            sb.AppendLine("    packageName    = 'Test'");
            sb.AppendLine("    url            = 'http://xyz/test.msi'");
            sb.AppendLine("    Url64bit       = 'http://xyz/test64.msi'");
            sb.AppendLine("    fileType       = \"msi\"");
            sb.AppendLine("    softwareName   = 'Test'");
            sb.AppendLine("    validExitCodes = @(0)");
            sb.AppendLine("}");
            sb.AppendLine("Install-ChocolateyPackage @packageArgs");
            return sb.ToString();
        }

        public static string GetExampleNuspec()
        {
            return "<?xml version=\"1.0\"?>" +
                   "<package xmlns=\"http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd\">" +
                   "  <metadata>" +
                   "    <id>azcopy</id>" +
                   "    <version>8.1.0</version>" +
                   "    <title>azcopy</title>" +
                   "    <authors>Microsoft</authors>" +
                   "    <owners>erichexter,jbpaux</owners>" +
                   "    <projectUrl>https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy</projectUrl>" +
                   "    <iconUrl>https://raw.githubusercontent.com/jbpaux/chocolatey-packages/master/azure-cli/azcopy-logo.png</iconUrl>" +
                   "    <requireLicenseAcceptance>false</requireLicenseAcceptance>" +
                   "    <description>AzCopy is a command-line utility designed for high-performance uploading, downloading, and copying data to and from Microsoft Azure Blob and File.</description>" +
                   "    <summary>The Azure CLI is Azure's command line experience for managing Azure resources.</summary>" +
                   "    <tags>azcopy azure admin</tags>" +
                   "    <packageSourceUrl>https://github.com/jbpaux/chocolatey-packages/tree/master/azcopy</packageSourceUrl>" +
                   "    <docsUrl>https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy</docsUrl>" +
                   "  </metadata>" +
                   "</package>";
        }
    }
}
