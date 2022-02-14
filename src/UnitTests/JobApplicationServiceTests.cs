using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Downloads;
using Scrap.JobDefinitions;
using Scrap.Jobs;
using Scrap.Jobs.Graphs;
using Scrap.Pages;
using Scrap.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests;

public class JobApplicationServiceTests
{
    private readonly ITestOutputHelper _output;

    public JobApplicationServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DownloadLinks_NormalFlow()
    {
        IResourceRepositoryConfiguration resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>();
        var resourceXPath = "//a/@href";
        var traversal = new[]
        {
            TestTools.PageMock("https://example.com/a", resourceXPath, "http://example.com/1.txt", "http://example.com/2.txt"),
            TestTools.PageMock("https://example.com/b", resourceXPath, "http://example.com/3.txt", "http://example.com/4.txt")
        }.ToAsyncEnumerable();

        var jobDto = new NewJobDto(
            null,
            resourceXPath,
            resourceRepoConfig,
            "https://example.com",
            null,
            null,
            null,
            null,
            ResourceType.DownloadLink,
            null,
            null);
        var pageRetrieverMock = new Mock<IPageRetriever>();
        var graphSearch = Mock.Of<IGraphSearch>(x =>
            x.SearchAsync(It.IsAny<Uri>(), It.IsAny<Func<Uri, Task<IPage>>>(),
                It.IsAny<Func<IPage, IAsyncEnumerable<Uri>>>()) == traversal);
        var streamProviderMock = new Mock<IDownloadStreamProvider>();
        streamProviderMock.Setup(y => y.GetStreamAsync(It.IsAny<Uri>()))
            .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("some text goes here!..")));
        var jobFactoryMock = new Mock<IJobFactory>();
        jobFactoryMock.Setup(x => x.CreateAsync(It.IsAny<NewJobDto>()))
            .ReturnsAsync(new Job(jobDto));

        var pageMarkerRepositoryMock = new Mock<IPageMarkerRepository>();
        var resourceRepositoryMock = new Mock<IResourceRepository>();
        resourceRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<ResourceInfo>())).ReturnsAsync(false);
        var jobServicesFactoryMock = new Mock<IJobServicesFactory>();
        jobServicesFactoryMock.Setup(x => x.GetDownloadStreamProvider(It.IsAny<Job>()))
            .Returns(streamProviderMock.Object);
        jobServicesFactoryMock.Setup(x => x.GetHttpPageRetriever(It.IsAny<Job>()))
            .Returns(pageRetrieverMock.Object);
        jobServicesFactoryMock.Setup(x => x.GetPageMarkerRepository(It.IsAny<Job>()))
            .Returns(pageMarkerRepositoryMock.Object);
        jobServicesFactoryMock.Setup(x => x.GetResourceRepositoryAsync(It.IsAny<Job>()))
            .ReturnsAsync(resourceRepositoryMock.Object);
        var loggerMock = new Mock<ILogger<JobApplicationService>>();
        loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback((LogLevel _, EventId _, object state, Exception? _, object _) => _output.WriteLine(state.ToString()));
        var service = new JobApplicationService(
            graphSearch,
            jobServicesFactoryMock.Object,
            jobFactoryMock.Object,
            loggerMock.Object);

        await service.ScrapAsync(jobDto);

        resourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("http://example.com/1.txt")), It.IsAny<Stream>()),
            Times.Once);
        resourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("http://example.com/2.txt")), It.IsAny<Stream>()),
            Times.Once);
        resourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("http://example.com/3.txt")), It.IsAny<Stream>()),
            Times.Once);
        resourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("http://example.com/4.txt")), It.IsAny<Stream>()),
            Times.Once);
        
        pageMarkerRepositoryMock.Verify(
            x => x.ExistsAsync(new Uri("https://example.com/a")),
            Times.Never);
        pageMarkerRepositoryMock.Verify(
            x => x.UpsertAsync(new Uri("https://example.com/a")),
            Times.Once);
        pageMarkerRepositoryMock.Verify(
            x => x.UpsertAsync(new Uri("https://example.com/b")),
            Times.Once);
    }
    [Fact]
    public async Task DownloadLinks()
    {
        IResourceRepositoryConfiguration resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>();
        var resourceXPath = "//a/@href";
        var traversal = new[]
        {
            TestTools.PageMock("https://example.com/a", resourceXPath, "http://example.com/1.txt", "http://example.com/2.txt"),
            TestTools.PageMock("https://example.com/b", resourceXPath, "http://example.com/3.txt", "http://example.com/4.txt")
        }.ToAsyncEnumerable();

        var jobDto = new NewJobDto(
            null,
            resourceXPath,
            resourceRepoConfig,
            "https://example.com",
            null,
            null,
            null,
            null,
            ResourceType.DownloadLink,
            null,
            null);
        var pageRetrieverMock = new Mock<IPageRetriever>();
        var graphSearch = Mock.Of<IGraphSearch>(x =>
            x.SearchAsync(It.IsAny<Uri>(), It.IsAny<Func<Uri, Task<IPage>>>(),
                It.IsAny<Func<IPage, IAsyncEnumerable<Uri>>>()) == traversal);
        var streamProviderMock = new Mock<IDownloadStreamProvider>();
        streamProviderMock.Setup(y => y.GetStreamAsync(It.IsAny<Uri>()))
            .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("some text goes here!..")));
        var jobFactoryMock = new Mock<IJobFactory>();
        jobFactoryMock.Setup(x => x.CreateAsync(It.IsAny<NewJobDto>()))
            .ReturnsAsync(new Job(jobDto));

        var pageMarkerRepositoryMock = new Mock<IPageMarkerRepository>();
        var resourceRepositoryMock = new Mock<IResourceRepository>();
        resourceRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<ResourceInfo>())).ReturnsAsync(false);
        var jobServicesFactoryMock = new Mock<IJobServicesFactory>();
        jobServicesFactoryMock.Setup(x => x.GetDownloadStreamProvider(It.IsAny<Job>()))
            .Returns(streamProviderMock.Object);
        jobServicesFactoryMock.Setup(x => x.GetHttpPageRetriever(It.IsAny<Job>()))
            .Returns(pageRetrieverMock.Object);
        jobServicesFactoryMock.Setup(x => x.GetPageMarkerRepository(It.IsAny<Job>()))
            .Returns(pageMarkerRepositoryMock.Object);
        jobServicesFactoryMock.Setup(x => x.GetResourceRepositoryAsync(It.IsAny<Job>()))
            .ReturnsAsync(resourceRepositoryMock.Object);
        var loggerMock = new Mock<ILogger<JobApplicationService>>();
        loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback((LogLevel _, EventId _, object state, Exception? _, object _) => _output.WriteLine(state.ToString()));
        var service = new JobApplicationService(
            graphSearch,
            jobServicesFactoryMock.Object,
            jobFactoryMock.Object,
            loggerMock.Object);

        await service.ScrapAsync(jobDto);

        resourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("http://example.com/1.txt")), It.IsAny<Stream>()),
            Times.Once);
        resourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("http://example.com/2.txt")), It.IsAny<Stream>()),
            Times.Once);
        resourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("http://example.com/3.txt")), It.IsAny<Stream>()),
            Times.Once);
        resourceRepositoryMock.Verify(
            x => x.UpsertAsync(It.Is<ResourceInfo>(y => y.ResourceUrl == new Uri("http://example.com/4.txt")), It.IsAny<Stream>()),
            Times.Once);
        
        pageMarkerRepositoryMock.Verify(
            x => x.ExistsAsync(new Uri("https://example.com/a")),
            Times.Never);
        pageMarkerRepositoryMock.Verify(
            x => x.UpsertAsync(new Uri("https://example.com/a")),
            Times.Once);
        pageMarkerRepositoryMock.Verify(
            x => x.UpsertAsync(new Uri("https://example.com/b")),
            Times.Once);
    }
}
