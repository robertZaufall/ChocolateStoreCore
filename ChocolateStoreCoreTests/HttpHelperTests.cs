using ChocolateStoreCore.Helpers;
using Microsoft.Extensions.DependencyInjection;
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
        [Fact]
        public void GetPackageIdFromTemplate()
        {
            // Arrange
            var id = "test";
            var settings = new Settings { ApiPackageRequest = "test1{0}test2" };

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
            var settings = new Settings { ApiPackageRequestWithVersion = "test1{0}test2{1}test3" };

            var sut = new HttpHelper(settings, null, null, null);

            // Act
            var test = sut.GetPackageIdInfoTemplate(id, version);

            // Assert
            test.Should().Be("test1" + id + "test2" + version + "test3");
        }
    }
}
