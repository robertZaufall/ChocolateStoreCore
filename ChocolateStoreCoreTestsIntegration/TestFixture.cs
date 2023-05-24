using ChocolateStoreCore.Helpers;
using ChocolateStoreCore.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Xml.Linq;
using ChocolateStoreCore;
using Moq.Protected;
using Moq;
using System.Net;
using AutoFixture.AutoMoq;
using AutoFixture;

namespace ChocolateStoreCoreTestsIntegration
{
    [ExcludeFromCodeCoverage]
    public class TestFixture : IDisposable
    {
        public readonly string ResourceDirName = "Resources";

        public ISettings Settings;
        public IFileHelper FileHelper;
        //public IHttpHelper HttpHelper;
        //public IChocolateyHelper ChocolateyHelper;
        private readonly List<string> Paths = new();


        public TestFixture()
        {
            string root = Directory.GetParent(AppContext.BaseDirectory).FullName;
            var configuration = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(root)
                .AddJsonFile("appsettings.json", false)
                .Build();

            Settings = new Settings(ChocolateStoreCore.Models.Settings.GetConfiguration(), root);
            FileHelper = new FileHelper();
        }

        public IHttpHelper GetHttpHelper(Mock<HttpMessageHandler> handlerMock)
           => new HttpHelper(Settings, FileHelper, GetHttpClientFactoryMock(handlerMock), null);

        public IChocolateyHelper GetChocolateyHelper(Mock<HttpMessageHandler> handlerMock)
            => new ChocolateyHelper(Settings, FileHelper, GetHttpHelper(handlerMock), null);

        public IChocolateyHelper GetChocolateyHelper()
            => new ChocolateyHelper(Settings, FileHelper, null, null);

        public HttpResponseMessage DummyResponse() => new HttpResponseMessage();

        public IHttpClientFactory GetHttpClientFactoryMock(Mock<HttpMessageHandler> handlerMock)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient("chocolatey").ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object);
            return services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        }

        public string GetTemp()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            Paths.Add(tempPath);
            return tempPath;
        }

        public void Dispose()
        {
            Paths.ForEach(x =>
            {
                try
                {
                    if (Directory.Exists(x))
                    {
                        Directory.Delete(x, true);
                    }
                }
                catch (Exception)
                {
                }
            });
        }
    }
}
