using ChocolateStoreCore.Helpers;
using ChocolateStoreCore.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace ChocolateyStoreCoreTestsE2E
{
    [ExcludeFromCodeCoverage]
    public class TestFixture : IDisposable
    {
        public ISettings Settings;
        public IFileHelper FileHelper;
        public IHttpHelper HttpHelper;
        public IChocolateyHelper ChocolateyHelper;
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

            IServiceCollection services = new ServiceCollection();
            ServiceHelper.AddHttpClientFactoryToServiceCollection(Settings, ref services, "chocolatey");
            var serviceProvider = services.BuildServiceProvider();

            HttpHelper = new HttpHelper(Settings, FileHelper, serviceProvider.GetService<IHttpClientFactory>(), null);
            ChocolateyHelper = new ChocolateyHelper(Settings, FileHelper, HttpHelper, null);
        }

        public string GetTemp()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "ChocolateStoreCore", Guid.NewGuid().ToString());
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