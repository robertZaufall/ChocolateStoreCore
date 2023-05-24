using ChocolateStoreCore.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateStoreCoreTests
{
    [ExcludeFromCodeCoverage]
    public class HttpHelperTests : IClassFixture<TestFixture>
    {
        readonly TestFixture _fixture;
        private readonly Mock<ISettings> _settingsMock = new Mock<ISettings>();
        private readonly Mock<IFileHelper> _fileHelperMock = new Mock<IFileHelper>();
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        public HttpHelperTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void GetPackageIdFromTemplate()
        {
            // Arrange
            var id = "test";
            var settings = new Settings("") { ApiPackageRequest = "test1{0}test2" };

            var sut = new HttpHelper(settings, null, null, null);

            // Act
            var test = sut.GetPackageIdInfoTemplate(id);

            // Assert
            test.Should().Be("test1" + id + "test2");
        }

        [Fact]
        public void GetPackageIdInfoTemplateWithVersion()
        {
            // Arrange
            var id = "test";
            var version = "version";
            var settings = new Settings("") { ApiPackageRequestWithVersion = "test1{0}test2{1}test3" };

            var sut = new HttpHelper(settings, null, null, null);

            // Act
            var test = sut.GetPackageIdInfoTemplate(id, version);

            // Assert
            test.Should().Be("test1" + id + "test2" + version + "test3");
        }

        [Fact]
        public void GetMetadataForPackageId()
        {
            // Arrange
            var settings = new Settings("")
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
            var fileHelper = new Mock<IFileHelper>(MockBehavior.Strict);
            fileHelper.Setup(_ => _.FileCreate(It.IsAny<string>(), It.IsAny<Stream>())).Returns(true).Verifiable();

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("ABC")
                })
                .Verifiable();
            var httpClientFactoryMock = _fixture.GetHttpClientFactoryMock(handlerMock);

            var sut = new HttpHelper(new Settings(""), fileHelper.Object, _fixture.GetHttpClientFactoryMock(handlerMock), null);

            var filePath = @"1:\234";

            // Act
            var result = sut.DownloadFile("http://test", filePath);

            // Assert
            result.Should().Be(filePath);
        }

        [Fact]
        public async Task CheckUrl_WhenStatusCodeIsOk_ShouldReturnRequestUri()
        {
            //Arrange
            var url = "http://www.google.com";
            var expectedValue = "http://www.google.com/";
            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            responseMessage.RequestMessage = new HttpRequestMessage();
            responseMessage.RequestMessage.RequestUri = new System.Uri(expectedValue);

            var fakeHandler = new Mock<HttpMessageHandler>();
            fakeHandler.Protected()
                       .Setup<Task<HttpResponseMessage>>(
                          "SendAsync",
                          ItExpr.IsAny<HttpRequestMessage>(),
                          ItExpr.IsAny<CancellationToken>()
                       )
                       .ReturnsAsync(responseMessage);

            var client = new HttpClient(fakeHandler.Object);
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var target = new HttpHelper(_settingsMock.Object, _fileHelperMock.Object, _httpClientFactoryMock.Object, new Logger<HttpHelper>(new NullLoggerFactory()));

            //Act
            var result = target.CheckUrl(url);

            //Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public async Task CheckUrl_WhenStatusCodeIsNotOk_ShouldReturnNull()
        {
            //Arrange
            var url = "http://www.google.com";

            var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);

            var fakeHandler = new Mock<HttpMessageHandler>();
            fakeHandler.Protected()
                       .Setup<Task<HttpResponseMessage>>(
                          "SendAsync",
                          ItExpr.IsAny<HttpRequestMessage>(),
                          ItExpr.IsAny<CancellationToken>()
                       )
                       .ReturnsAsync(responseMessage);

            var client = new HttpClient(fakeHandler.Object);
            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            var target = new HttpHelper(_settingsMock.Object, _fileHelperMock.Object, _httpClientFactoryMock.Object, new Logger<HttpHelper>(new NullLoggerFactory()));

            //Act
            var result = target.CheckUrl(url);

            //Assert
            Assert.Null(result);
        }

    }
}
