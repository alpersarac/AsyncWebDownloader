using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncWebDownloader.Models;

namespace AsyncWebDownloader.Services
{
    public interface IPageDownloader
    {
        Task<DownloadResult> DownloadAsync(string url, string outputDir, CancellationToken ct);
    }
}
