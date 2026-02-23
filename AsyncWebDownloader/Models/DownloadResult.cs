using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncWebDownloader.Models
{
    public sealed record DownloadResult(
        string Url,
        bool Success,
        int? StatusCode,
        long? Bytes,
        string? SavedAs,
        string? Error,
        TimeSpan Duration
    );
}
