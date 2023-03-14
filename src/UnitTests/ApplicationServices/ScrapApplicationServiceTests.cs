using Moq;
using Scrap.Application.Scrap;
using Scrap.Common;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class ScrapApplicationServiceTests
{
    private readonly ITestOutputHelper _output;

    public ScrapApplicationServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ScrapAsync()
    {
        var jobDto = JobDtoBuilder.Build(ResourceType.DownloadLink);

        var scrapDownloadsServiceMock = new Mock<IScrapDownloadsService>();
        scrapDownloadsServiceMock.Setup(x => x.DownloadLinksAsync(It.IsAny<JobDto>())).Returns(Task.CompletedTask);
        var scrapTextServiceMock = new Mock<IScrapTextService>();
        scrapTextServiceMock.Setup(x => x.ScrapTextAsync(It.IsAny<JobDto>())).Returns(Task.CompletedTask);

        var service = new ScrapApplicationService(
            Mock.Of<IAsyncFactory<JobDto, Job>>(),
            scrapDownloadsServiceMock.Object,
            scrapTextServiceMock.Object);

        await service.ScrapAsync(jobDto);

        scrapDownloadsServiceMock.Verify(x => x.DownloadLinksAsync(It.IsAny<JobDto>()), Times.Once);
        scrapTextServiceMock.Verify(x => x.ScrapTextAsync(It.IsAny<JobDto>()), Times.Never);
    }

    [Fact]
    public async Task ScrapAsync_Texts()
    {
        var jobDto = JobDtoBuilder.Build(ResourceType.Text);

        var scrapDownloadsServiceMock = new Mock<IScrapDownloadsService>();
        scrapDownloadsServiceMock.Setup(x => x.DownloadLinksAsync(It.IsAny<JobDto>())).Returns(Task.CompletedTask);
        var scrapTextServiceMock = new Mock<IScrapTextService>();
        scrapTextServiceMock.Setup(x => x.ScrapTextAsync(It.IsAny<JobDto>())).Returns(Task.CompletedTask);

        var service = new ScrapApplicationService(
            Mock.Of<IAsyncFactory<JobDto, Job>>(),
            scrapDownloadsServiceMock.Object,
            scrapTextServiceMock.Object);

        await service.ScrapAsync(jobDto);

        scrapDownloadsServiceMock.Verify(x => x.DownloadLinksAsync(It.IsAny<JobDto>()), Times.Never);
        scrapTextServiceMock.Verify(x => x.ScrapTextAsync(It.IsAny<JobDto>()), Times.Once);
    }
}
