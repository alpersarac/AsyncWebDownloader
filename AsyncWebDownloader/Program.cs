
using AsyncWebDownloader.Options;
using AsyncWebDownloader.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;

static IAsyncPolicy<HttpResponseMessage> BuildRetryPolicy(int maxRetries)
{
    return Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => (int)r.StatusCode >= 500)
        .RetryAsync(maxRetries);
}
static IAsyncPolicy<HttpResponseMessage> BuildCircuitBreakerPolicy()
{
    return Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => (int)r.StatusCode >= 500)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(10)
        );
}
var options = new AppOptions
{
    Urls = 
    [
        "https://www.entaingroup.com/about-entain/",
        "https://www.entaingroup.com/news-insights/latest-news/",
        "https://www.wien.info/en/see-do/sights-from-a-to-z/st-stephens-cathedral-359690",
        "https://www.wien.info/en/now-on/eurovision-song-contest"

    ],
    MaxConcurrency=5,
    TimeoutSeconds=20,
    MaxRetries=2,
    OutputDirectory="output"
};

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("Cancelation Requested ctrl+c");

};

var builder = Host.CreateApplicationBuilder();
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss";
}
);

builder.Services.AddSingleton(options);
builder.Services.AddHttpClient<IPageDownloader, PageDownloader>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSeconds));
})
.AddPolicyHandler(BuildRetryPolicy(options.MaxRetries))
.AddPolicyHandler(BuildCircuitBreakerPolicy());

builder.Services.AddSingleton<DownloadCoordinator>();

var host = builder.Build();

var coordinator = host.Services.GetRequiredService<DownloadCoordinator>();
var results = await coordinator.RunAsync(options, cts.Token);

foreach (var r in results.OrderByDescending(r => r.Success))
{
    var status = r.Success ? "OK  " : "FAIL";
    var extra = r.Success
        ? $"{r.SavedAs} ({r.Bytes} bytes)"
        : r.Error;

    Console.WriteLine($"{status} | {r.Duration.TotalMilliseconds,6:0} ms | {r.Url} | {extra}");
}

Console.WriteLine();
Console.WriteLine($"Success: {results.Count(r => r.Success)}/{results.Count}");