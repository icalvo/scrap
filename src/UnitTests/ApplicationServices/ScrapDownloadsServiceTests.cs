using Moq;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

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
            new PageMock("https://example.com/a").ResourceLinks(
                MockBuilder.ResourceXPath,
                "https://example.com/1.txt",
                "https://example.com/2.txt"),
            new PageMock("https://example.com/b").ResourceLinks(
                MockBuilder.ResourceXPath,
                "https://example.com/3.txt",
                "https://example.com/4.txt"));
        builder.ResourceRepositoryMock.Setup(x => x.Type).Returns("FileSystemRepository");
        var jobDto = builder.BuildJobDto(ResourceType.DownloadLink);
        var service = builder.BuildScrapDownloadsService(jobDto);

        await service.DownloadLinksAsync(jobDto);

        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("https://example.com/1.txt")),
                It.IsAny<Stream>()),
            Times.Once);
        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("https://example.com/2.txt")),
                It.IsAny<Stream>()),
            Times.Once);
        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("https://example.com/3.txt")),
                It.IsAny<Stream>()),
            Times.Once);
        builder.ResourceRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("https://example.com/4.txt")),
                It.IsAny<Stream>()),
            Times.Once);

        builder.PageMarkerRepositoryMock.Verify(x => x.ExistsAsync(new Uri("https://example.com/a")), Times.Never);
        builder.PageMarkerRepositoryMock.Verify(x => x.UpsertAsync(new Uri("https://example.com/a")), Times.Once);
        builder.PageMarkerRepositoryMock.Verify(x => x.UpsertAsync(new Uri("https://example.com/b")), Times.Once);
    }
}
