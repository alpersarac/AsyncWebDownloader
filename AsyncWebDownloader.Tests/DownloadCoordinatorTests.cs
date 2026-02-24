using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncWebDownloader.Tests
{
    using AsyncWebDownloader.Models;
    using AsyncWebDownloader.Options;
    using AsyncWebDownloader.Services;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;

    public class DownloadCoordinatorTests
    {
        [Fact]
        public async Task RunAsync_ShouldReturnSuccess_ForAllUrls()
        {
            var urls = new List<string>
            {
                "https://a.com",
                "https://b.com"
            };

            var options = new AppOptions
            {
                Urls = urls,
                MaxConcurrency = 2,
                OutputDirectory = "output"
            };

            var mockDownloader = new Mock<IPageDownloader>();

            mockDownloader
                .Setup(d => d.DownloadAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((string url, string _, CancellationToken _) =>
                    new DownloadResult(
                        url,
                        true,
                        200,
                        100,
                        "file.html",
                        null,
                        TimeSpan.Zero));

            var coordinator = new DownloadCoordinator(
                mockDownloader.Object,
                NullLogger<DownloadCoordinator>.Instance);

            var result = await coordinator.RunAsync(options, CancellationToken.None);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(r => r.Success);
        }

        [Fact]
        public async Task RunAsync_ShouldReturnFailures_WhenDownloaderFails()
        {
            var mockDownloader = new Mock<IPageDownloader>();

            mockDownloader
                .Setup(d => d.DownloadAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DownloadResult(
                    "url",
                    false,
                    500,
                    null,
                    null,
                    "error",
                    TimeSpan.Zero));

            var options = new AppOptions
            {
                Urls = new List<string> { "https://fail.com" },
                OutputDirectory = "output"
            };

            var coordinator = new DownloadCoordinator(
                mockDownloader.Object,
                NullLogger<DownloadCoordinator>.Instance);

            var result = await coordinator.RunAsync(options, CancellationToken.None);

            result.Should().ContainSingle();
            result.First().Success.Should().BeFalse();
        }

    }
}
