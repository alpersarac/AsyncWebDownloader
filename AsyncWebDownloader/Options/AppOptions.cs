using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncWebDownloader.Options
{
    public sealed class AppOptions
    {
        public required List<string> Urls { get; set; }
        public int MaxConcurrency { get; set; } = 5;
        public int TimeoutSeconds { get; set; } = 20;
        public int MaxRetries { get; set; } = 2;
        public string OutputDirectory { get; set; } = "output";
    }
}
