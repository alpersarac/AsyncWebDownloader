using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncWebDownloader.Models;
using AsyncWebDownloader.Options;
using Microsoft.Extensions.Logging;

namespace AsyncWebDownloader.Services
{
    public class DownloadCoordinator
    {
        private readonly IPageDownloader _pageDownloader;
        private readonly ILogger _logger;
        public DownloadCoordinator(IPageDownloader pageDownloader, ILogger<DownloadCoordinator> logger)
        {
            _pageDownloader = pageDownloader;
            _logger = logger;
        }
        public async Task<IReadOnlyList<DownloadResult>> RunAsync(AppOptions appOptions, CancellationToken ct)
        {
            if (appOptions.Urls.Count == 0)
                throw new ArgumentException("No Url");
            var maxConcurrency = Math.Max(1, appOptions.MaxConcurrency);

            _logger.LogInformation("Starting downloads. Urls={Count}, MaxConcurrency={MaxConc}, Out={Out}",
                appOptions.Urls.Count, maxConcurrency, appOptions.OutputDirectory);

            var sw = Stopwatch.StartNew();

            using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

            var tasks = appOptions.Urls.Select(async url =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    return await _pageDownloader.DownloadAsync(url, appOptions.OutputDirectory, ct);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();

            var result = await Task.WhenAll(tasks);
            sw.Stop();

            var ok = result.Count(r => r.Success);
            var fail = result.Length - ok;
            var totalBytes = result.Where(r => r.Bytes.HasValue).Sum(r => r.Bytes!.Value);

            _logger.LogInformation("Done, Success={Ok}, Failed={Fail}, TotalBytes{Bytes}, ElapsedMs={ElapsedMs}",
                ok, fail, totalBytes, (long)sw.Elapsed.TotalMilliseconds);

            return result;
        }
    }
}
