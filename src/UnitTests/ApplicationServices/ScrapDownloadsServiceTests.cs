using Moq;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
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
        var builder = new ScrapDownloadsServiceMockBuilder(_output);

        builder.SetupTraversal(
            new PageMock("https://example.com/a").ResourceLinks(
                JobDtoBuilder.ResourceXPath,
                "https://example.com/1.txt",
                "https://example.com/2.txt"),
            new PageMock("https://example.com/b").ResourceLinks(
                JobDtoBuilder.ResourceXPath,
                "https://example.com/3.txt",
                "https://example.com/4.txt"));
        builder.ResourceRepositoryMock.Setup(x => x.Type).Returns("FileSystemRepository");
        var jobDto = JobDtoBuilder.Build(ResourceType.DownloadLink);
        builder.JobFactoryMock.SetupFactory(new Job(jobDto));
        var service = builder.BuildScrapDownloadsService();

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


    [Fact]
    public async Task DownloadLinksAsync_Other()
    {
        var builder = new ScrapDownloadsServiceMockBuilder(_output);

        builder.SetupTraversal2(
            new PageMock("https://example.com/a").ResourceLinks(
                JobDtoBuilder.ResourceXPath,
                "https://example.com/1.txt",
                "https://example.com/2.txt"),
            new PageMock("https://example.com/b").ResourceLinks(
                JobDtoBuilder.ResourceXPath,
                "https://example.com/3.txt",
                "https://example.com/4.txt"));
        builder.ResourceRepositoryMock.Setup(x => x.Type).Returns("FileSystemRepository");
        var jobDto = JobDtoBuilder.Build(ResourceType.DownloadLink);
        builder.JobFactoryMock.SetupFactory(new Job(jobDto));
        var service = builder.BuildScrapDownloadsService();

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
