using ChocolateStoreCore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;

namespace ChocolateStoreCore.Helpers
{
    public interface IHttpHelper
    {
        string DownloadFile(string url, string destination);
        string GetMetadataForPackageId(string packageId);
        string GetPackageIdInfoTemplate(string packageId);
        string GetPackageIdInfoTemplate(string packageId, string version);
        string CheckUrl(string url);
    }

    [ExcludeFromCodeCoverage]
    public class HttpHelper : IHttpHelper
    {
        private readonly ISettings _settings;
        private readonly ILogger<HttpHelper> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpHelper(ISettings settings, IHttpClientFactory httpClientFactory, ILogger<HttpHelper> logger)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? new Logger<HttpHelper>(new NullLoggerFactory());
            _httpClientFactory = httpClientFactory;
        }

        public string GetPackageIdInfoTemplate(string packageId)
        {
            return String.Format(_settings.ApiPackageRequest, packageId);
        }

        public string GetPackageIdInfoTemplate(string packageId, string version)
        {
            return String.Format(_settings.ApiPackageRequestWithVersion, packageId, version);
        }

        public string GetMetadataForPackageId(string packageId)
        {
            var url = new Uri(new Uri(_settings.ApiUrl), _settings.ApiPath + GetPackageIdInfoTemplate(packageId));

            try
            {
                using (var request = _httpClientFactory.CreateClient("chocolatey"))
                {
                    var response = request.GetAsync(url).Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var content = new StreamReader(response.Content.ReadAsStreamAsync().Result).ReadToEnd();
                        _logger.LogInformation("Download metadata for {packageId} done.", packageId);
                        return content;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get metadata for {packageId}", packageId);
                return null;
            }
        }

        public string CheckUrl(string url)
        {
            try
            {
                using (var request = _httpClientFactory.CreateClient("chocolatey"))
                {
                    HttpRequestMessage reqMessage = new(HttpMethod.Head, url);
                 
                    var response = request.SendAsync(reqMessage).Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var requestUri = response.RequestMessage?.RequestUri;
                        return requestUri?.ToString();
                    }
                    else
                    {
                        _logger.LogError("Failed to check url {url}, http statuscode {statusCode}", url, response.StatusCode.ToString());
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check url {url}", url);
                return null;
            }
        }

        public string DownloadFile(string url, string filePath)
        {
            try
            {
                using (var request = _httpClientFactory.CreateClient("chocolatey"))
                {
                    var response = request.GetAsync(new Uri(url)).Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var requestUri = response.RequestMessage?.RequestUri;
                        using (var fs = File.Create(filePath))
                        {
                            response.Content.ReadAsStream().CopyTo(fs);
                        }
                        _logger.LogInformation("Download from {url} done.", url);
                        return filePath;
                    }
                    else
                    {
                        _logger.LogError("Failed to download {url}, http statuscode {statusCode}", url, response.StatusCode.ToString());
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download {url}", url);
                return null;
            }
        }
    }
}
