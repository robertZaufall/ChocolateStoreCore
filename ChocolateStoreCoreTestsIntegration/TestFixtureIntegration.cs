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

namespace ChocolateStoreCoreTestsIntegration
{
    [ExcludeFromCodeCoverage]
    public class TestFixtureIntegration : IDisposable
    {
        public readonly string ResourceDirName = "Resources";

        public ISettings Settings;
        public IFileHelper FileHelper;
        //public IHttpHelper HttpHelper;
        //public IChocolateyHelper ChocolateyHelper;
        private readonly List<string> Paths = new();

        public TestFixtureIntegration()
        {
            var configuration = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            Settings = new Settings(ChocolateStoreCore.Models.Settings.GetConfiguration());
            FileHelper = new FileHelper();
        }

        public IHttpHelper GetHttpHelper(Mock<HttpMessageHandler> handlerMock)
           => new HttpHelper(Settings, GetHttpClientMock(handlerMock), null);

        public IChocolateyHelper GetChocolateyHelper(Mock<HttpMessageHandler> handlerMock)
            => new ChocolateyHelper(Settings, FileHelper, GetHttpHelper(handlerMock), null);

        public IChocolateyHelper GetChocolateyHelper()
            => new ChocolateyHelper(Settings, FileHelper, null, null);

        public HttpResponseMessage DummyResponse() => new HttpResponseMessage();

        public IHttpClientFactory GetHttpClientMock(Mock<HttpMessageHandler> handlerMock)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddHttpClient("chocolatey", client =>
            {
                client.BaseAddress = new Uri(Settings.ApiUrl + Settings.ApiPath);
                client.DefaultRequestHeaders.Add("User-Agent", "Chocolatey Core");
                client.Timeout = TimeSpan.FromMinutes(Settings.HttpTimeoutOverAll);
            })
            .AddPolicyHandlerFromRegistry("regularTimeout")
            .AddPolicyHandlerFromRegistry("waitAndRetryPolicy")
            .SetHandlerLifetime(TimeSpan.FromMinutes(Settings.HttpHandlerLifetime))
            .ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object);

            var httpClient = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
            return httpClient;
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
