using ChocolateStoreCore.Helpers;
using ChocolateStoreCore.Models;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace ChocolateStoreCore
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var root = FileHelper.GetApplicationRoot();
            ISettings settings = new Settings(Settings.GetConfiguration(), root);
            var serviceProvider = ConfigureServices(settings).BuildServiceProvider();

            ArgsOptions options = null;
            Parser.Default.ParseArguments<ArgsOptions>(args).WithParsed<ArgsOptions>(o => { options = o; });

            if (options != null && options.Purge)
            {
                await serviceProvider.GetService<IApp>().Purge(options?.WhatIf ?? false);
            }
            else
            {
                await serviceProvider.GetService<IApp>().Run(options?.WhatIf ?? false);
            }
        }

        public static Logger GetLogger(ISettings settings)
        {
            LogEventLevel logLevel = settings.GetLogLevel();
            return new LoggerConfiguration()
                 .WriteTo.Console(Serilog.Events.LogEventLevel.Debug, theme: AnsiConsoleTheme.Literate)
                 .WriteTo.File(settings.LogFile, rollingInterval: RollingInterval.Day)
                 .MinimumLevel.Is(logLevel)
                 .Enrich.FromLogContext()
                 .CreateLogger();
        }

        public static IServiceCollection ConfigureServices(ISettings settings)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton(settings);
            services.AddSingleton(LoggerFactory.Create(builder => builder.AddSerilog(GetLogger(settings), dispose: true)));
            services.AddLogging();

            services.AddTransient<IApp, App>();
            services.AddTransient<IPackageCacher, PackageCacher>();
            services.AddTransient<IFileHelper, FileHelper>();
            services.AddTransient<IHttpHelper, HttpHelper>();
            services.AddTransient<IChocolateyHelper, ChocolateyHelper>();

            ServiceHelper.AddHttpClientFactoryToServiceCollection(settings, ref services, name: "chocolatey");

            return services;
        }
    }
}
