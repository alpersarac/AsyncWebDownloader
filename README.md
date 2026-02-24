

# Async Web Downloader
![24 02 2026_20 34 53_REC](https://github.com/user-attachments/assets/7ba5c76d-5f3b-4108-b16d-d3da387af069)


## .NET console application that downloads multiple web pages asynchronously.

# Features

- Async/await based parallel downloads
- Configurable max concurrency
- Retry & circuit breaker (Polly)
- Cancellation (Ctrl+C)
- Structured logging
- Unit tests (xUnit, Moq)

## Architecture

- DownloadCoordinator: Coordinates concurrency
- PageDownloader: Download pages
- HttpClientFactory and Polly relaiable HTTP communication

The solution follows SOLID principles and clean separation of concerns.

## Configuration

Set urls and you are ready to go
Example:

```charp
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
