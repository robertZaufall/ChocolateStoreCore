using ChocolateStoreCore.Models;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Net.Http;

namespace ChocolateStoreCore.Helpers
{
    public static class ServiceHelper
    {
        public static void AddHttpClientFactoryToServiceCollection(ISettings settings, ref IServiceCollection services, string name)
        {
            var registry = services.AddPolicyRegistry();
            registry.Add("regularTimeout", Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(settings.HttpTimeout)));
            registry.Add("waitAndRetryPolicy", Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(settings.HttpRetries, retryAttempt => TimeSpan.FromSeconds(retryAttempt)));

            services.AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri(settings.ApiUrl + settings.ApiPath);
                client.DefaultRequestHeaders.Add("User-Agent", "Chocolatey Core");
                client.Timeout = TimeSpan.FromMinutes(settings.HttpTimeoutOverAll);
            })
            .AddPolicyHandlerFromRegistry("regularTimeout")
            .AddPolicyHandlerFromRegistry("waitAndRetryPolicy")
            .SetHandlerLifetime(TimeSpan.FromMinutes(settings.HttpHandlerLifetime))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = true });
        }
    }
}
