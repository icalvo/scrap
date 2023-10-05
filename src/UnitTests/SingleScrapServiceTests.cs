using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Application.Scrap;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit;

public class SingleScrapServiceTests
{
    private readonly ITestOutputHelper _output;

    public SingleScrapServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ExecuteJobAsync_Downloads()
    {
        var job = JobBuilder.Build(ResourceType.DownloadLink);

        var scrapDownloadsServiceMock = new Mock<IScrapDownloadsService>();
        scrapDownloadsServiceMock.Setup(x => x.DownloadLinksAsync(It.IsAny<Job>())).Returns(Task.CompletedTask);
        var scrapTextServiceMock = new Mock<IScrapTextService>();
        scrapTextServiceMock.Setup(x => x.ScrapTextAsync(It.IsAny<Job>())).Returns(Task.CompletedTask);
        var service = new SingleScrapService(
            scrapDownloadsServiceMock.Object,
            scrapTextServiceMock.Object,
            Mock.Of<ILogger<SingleScrapService>>());

        await service.ExecuteJobAsync("a", job);

        scrapDownloadsServiceMock.Verify(x => x.DownloadLinksAsync(It.IsAny<Job>()), Times.Once);
        scrapTextServiceMock.Verify(x => x.ScrapTextAsync(It.IsAny<Job>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteJobAsync_Texts()
    {
        var job = JobBuilder.Build(ResourceType.Text);

        var scrapDownloadsServiceMock = new Mock<IScrapDownloadsService>();
        scrapDownloadsServiceMock.Setup(x => x.DownloadLinksAsync(It.IsAny<Job>())).Returns(Task.CompletedTask);
        var scrapTextServiceMock = new Mock<IScrapTextService>();
        scrapTextServiceMock.Setup(x => x.ScrapTextAsync(It.IsAny<Job>())).Returns(Task.CompletedTask);
        var service = new SingleScrapService(
            scrapDownloadsServiceMock.Object,
            scrapTextServiceMock.Object,
            Mock.Of<ILogger<SingleScrapService>>());

        await service.ExecuteJobAsync("a", job);

        scrapDownloadsServiceMock.Verify(x => x.DownloadLinksAsync(It.IsAny<Job>()), Times.Never);
        scrapTextServiceMock.Verify(x => x.ScrapTextAsync(It.IsAny<Job>()), Times.Once);
    }
}
