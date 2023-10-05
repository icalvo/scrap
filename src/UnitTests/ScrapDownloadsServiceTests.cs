using Moq;
using Scrap.Domain;
using Scrap.Domain.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit;

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
        var builder = new ScrapDownloadsServiceMockBuilder(_output);

        builder.SetupTraversal(
            new PageMock("https://example.com/a").ResourceLinks(
                JobBuilder.ResourceXPath,
                "https://example.com/1.txt",
                "https://example.com/2.txt"),
            new PageMock("https://example.com/b").ResourceLinks(
                JobBuilder.ResourceXPath,
                "https://example.com/3.txt",
                "https://example.com/4.txt"));
        builder.ResourceRepositoryMock.Setup(x => x.Type).Returns("FileSystemRepository");
        var job = JobBuilder.Build(ResourceType.DownloadLink);
        var service = builder.BuildScrapDownloadsService();

        await service.DownloadLinksAsync(job);

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

        builder.VisitedPageRepositoryMock.Verify(x => x.ExistsAsync(new Uri("https://example.com/a")), Times.Never);
        builder.VisitedPageRepositoryMock.Verify(x => x.UpsertAsync(new Uri("https://example.com/a")), Times.Once);
        builder.VisitedPageRepositoryMock.Verify(x => x.UpsertAsync(new Uri("https://example.com/b")), Times.Once);
    }


    [Fact]
    public async Task DownloadLinksAsync_Other()
    {
        var builder = new ScrapDownloadsServiceMockBuilder(_output);

        builder.SetupTraversal2(
            new PageMock("https://example.com/a").ResourceLinks(
                JobBuilder.ResourceXPath,
                "https://example.com/1.txt",
                "https://example.com/2.txt"),
            new PageMock("https://example.com/b").ResourceLinks(
                JobBuilder.ResourceXPath,
                "https://example.com/3.txt",
                "https://example.com/4.txt"));
        builder.ResourceRepositoryMock.Setup(x => x.Type).Returns("FileSystemRepository");
        var job = JobBuilder.Build(ResourceType.DownloadLink);
        var service = builder.BuildScrapDownloadsService();

        await service.DownloadLinksAsync(job);

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

        builder.VisitedPageRepositoryMock.Verify(x => x.ExistsAsync(new Uri("https://example.com/a")), Times.Never);
        builder.VisitedPageRepositoryMock.Verify(x => x.UpsertAsync(new Uri("https://example.com/a")), Times.Once);
        builder.VisitedPageRepositoryMock.Verify(x => x.UpsertAsync(new Uri("https://example.com/b")), Times.Once);
    }
}
