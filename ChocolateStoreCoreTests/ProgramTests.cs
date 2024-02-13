using Microsoft.Extensions.DependencyInjection;

namespace ChocolateStoreCoreTests
{
    [ExcludeFromCodeCoverage]
    public class ProgramTests
    {
        private readonly Program _sut;
        private readonly ISettings _settings;

        public ProgramTests()
        {
            var settings = new Mock<ISettings>();
            settings.Setup(x => x.LogFile).Returns(Path.Combine(Path.GetTempPath(), "test.log"));
            settings.Setup(x => x.HttpHandlerLifetime).Returns(1);
            settings.Setup(x => x.HttpRetries).Returns(2);
            settings.Setup(x => x.HttpTimeout).Returns(3);
            _settings = settings.Object;

            _sut = new Program();
        }

/*        [Fact]
        public void ConfigureServicesTest()
        {
            // Arrange

            // Act
            var services = Program.ConfigureServices(_settings);

            // Assert
            services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(ISettings), _settings));
            services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IApp), typeof(App), ServiceLifetime.Transient));
            services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IPackageCacher), typeof(PackageCacher), ServiceLifetime.Transient));
            services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IHttpHelper), typeof(HttpHelper), ServiceLifetime.Transient));
            services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IFileHelper), typeof(FileHelper), ServiceLifetime.Transient));
            services.Should().ContainEquivalentOf(new ServiceDescriptor(typeof(IChocolateyHelper), typeof(ChocolateyHelper), ServiceLifetime.Transient));
        }*/

        [Fact]
        public void GetLoggerTest()
        {
            // Arrange

            // Act
            var logger = Program.GetLogger(_settings);

            // Assert
            logger.Should().NotBeNull();
            var logerType = logger.GetType();
            logerType.FullName.Should().Be("Serilog.Core.Logger");
            logerType.GetInterfaces().ToList().Select(x => (Tuple<string, string>)new(x.Name, x.FullName)).ToList()
                .Should().BeEquivalentTo(new List<(string, string)>
                {
                    ("ILogger", "Serilog.ILogger"),
                    ("ILogEventSink", "Serilog.Core.ILogEventSink"),
                    ("IDisposable", "System.IDisposable"),
                    ("IAsyncDisposable", "System.IAsyncDisposable")
                });
        }
    }
}
