using Moq;
using Scrap.Domain.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests;

public class ScrapDownloadsServiceTests
{
    private readonly ITestOutputHelper _output;

    public ScrapDownloadsServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DownloadLinksAsync_NormalFlow()
    {
        var builder = new MockBuilder(
            _output,
            TestTools.PageMock(
                "https://example.com/a",
                MockBuilder.LinkXPath, new[] { "https://example.com/b" },
                MockBuilder.ResourceXPath, new[] { "https://example.com/1.txt", "https://example.com/2.txt" },
                MockBuilder.ResourceXPath, new[] { "qwer", "asdf" }),
            TestTools.PageMock(
                "https://example.com/b",
                MockBuilder.LinkXPath, new[] { "https://example.com/a" },
                MockBuilder.ResourceXPath, new[] { "https://example.com/3.txt", "https://example.com/4.txt" },
                MockBuilder.ResourceXPath, new[] { "zxcv", "yuio" }));
        var jobDto = builder.BuildJobDto();
        var service = builder.BuildScrapDownloadsService(jobDto);

        await service.DownloadLinksAsync(jobDto);

        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("https://example.com/1.txt")),
                It.IsAny<Stream>()),
            Times.Once);
        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("https://example.com/2.txt")),
                It.IsAny<Stream>()),
            Times.Once);
        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("https://example.com/3.txt")),
                It.IsAny<Stream>()),
            Times.Once);
        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("https://example.com/4.txt")),
                It.IsAny<Stream>()),
            Times.Once);

        builder.PageMarkerRepositoryMock.Verify(
            x => x.ExistsAsync(new Uri("https://example.com/a")),
            Times.Never);
        builder.PageMarkerRepositoryMock.Verify(
            x => x.UpsertAsync(new Uri("https://example.com/a")),
            Times.Once);
        builder.PageMarkerRepositoryMock.Verify(
            x => x.UpsertAsync(new Uri("https://example.com/b")),
            Times.Once);
    }
}
