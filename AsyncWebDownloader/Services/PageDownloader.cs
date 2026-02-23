using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AsyncWebDownloader.Models;
using Microsoft.Extensions.Logging;

namespace AsyncWebDownloader.Services
{
    public sealed class PageDownloader : IPageDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PageDownloader> _logger;
        public PageDownloader(HttpClient httpClient, ILogger<PageDownloader> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<DownloadResult> DownloadAsync(string url, string outputDir, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);

                using var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);

                var statusCode = (int)response.StatusCode;

                if (!response.IsSuccessStatusCode) 
                {
                    var err = $"HTTP {statusCode} {response.ReasonPhrase}";
                    _logger.LogWarning("Download failed: {Url} -> {Status}", url, err);

                    return new DownloadResult(url,false,statusCode,null,null,err, sw.Elapsed);
                }

                Directory.CreateDirectory(outputDir);

                var fileName = BuildSafeFileName(url);
                var filePath = Path.Combine(outputDir, fileName);

                await using var httpStream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = File.Create(filePath);

                await httpStream.CopyToAsync(fileStream, ct);

                _logger.LogInformation("Downloaded {Url} -> {File} ({Bytes} bytes)");

                return new DownloadResult(url,true,statusCode,fileStream.Length,fileName,null,sw.Elapsed);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning("Cancelled: {Url}", url);
                return new DownloadResult(url, false, null, null, null, "Cancelled", sw.Elapsed);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "HTTP error while downloading: {Url}", url);
                return new DownloadResult(url, false, null, null, null, ex.Message, sw.Elapsed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while downloading: {Url}", url);
                return new DownloadResult(url, false, null, null, null, ex.Message, sw.Elapsed);
            }
        }
        private static string BuildSafeFileName(string url)
        {
            var uri = new Uri(url);
            var host = uri.Host.Replace(".", "_");

            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url)))[..8]
                .ToLowerInvariant();

            //I added guid for files to avoid conflicts in case of same url is being tried again
            return $"{host}_{hash}_{Guid.NewGuid():N}.html";
        }
    }
}
