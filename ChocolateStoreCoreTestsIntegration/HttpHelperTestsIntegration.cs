using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.DependencyInjection;
using Moq.Protected;
using System.Xml.Linq;

namespace ChocolateStoreCoreTestsIntegration
{
    [ExcludeFromCodeCoverage]
    public class HttpHelperTestsIntegration : IClassFixture<TestFixture>
    {
        readonly string _name = "chocolatey";
        readonly TestFixture _fixture;

        public HttpHelperTestsIntegration(TestFixture fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public void GetMetadataForPackageId()
        {
            // Arrange
            var settings = new Settings 
            { 
                ApiUrl = "https://abc.def.efg", 
                ApiPath = "/xyz/v12",
                ApiPackageRequest = "test1{0}test2",
                ApiPackageRequestWithVersion = "test1{0}test2{1}test3"
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent("ABC") })
                .Verifiable();

            var httpHelper = new HttpHelper(settings, null, _fixture.GetHttpClientFactoryMock(handlerMock), null);

            // Act
            var content = httpHelper.GetMetadataForPackageId("Test");

            // Assert
            content.Should().Be("ABC");
        }

        [Fact]
        public void DownloadFile()
        {
            // Arrange
            var settings = new Settings();

            //var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var fileHelper = new Mock<IFileHelper>(MockBehavior.Strict);
            fileHelper.Setup(_ => _.FileCreate(It.IsAny<string>(), It.IsAny<Stream>())).Returns(true);
            
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK, Content = new StringContent("ABC") })
                .Verifiable();
            var httpClientFactoryMock = _fixture.GetHttpClientFactoryMock(handlerMock);

            var sut = new HttpHelper(settings, fileHelper.Object, _fixture.GetHttpClientFactoryMock(handlerMock), null);

            var filePath = @"c:\Test";

            // Act
            var result = sut.DownloadFile("http://test", filePath);

            // Assert
            result.Should().Be(filePath);
        }
    }
}
