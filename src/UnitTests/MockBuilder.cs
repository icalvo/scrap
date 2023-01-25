using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Application;
using Scrap.Application.Scrap;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Xunit.Abstractions;

namespace Scrap.Tests;

public class MockBuilder
{
    public const string LinkXPath = "//a/@href";
    public const string ResourceXPath = "//img/@src";
    private readonly ITestOutputHelper _output;
    private readonly Mock<IFactory<Job, IDownloadStreamProvider>> _streamProviderFactoryMock = new();
    private Mock<IDownloadStreamProvider> _streamProviderMock = new();

    public MockBuilder(ITestOutputHelper output, params IPage[] traversal)
    {
        _output = output;
        Traversal = traversal;
        LinkCalculatorFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(LinkCalculatorMock.Object);
        PageMarkerRepositoryFactoryMock.Setup(x => x.Build()).Returns(PageMarkerRepositoryMock.Object);
        PageMarkerRepositoryFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(PageMarkerRepositoryMock.Object);
        PageRetrieverFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(PageRetrieverMock.Object);
        ResourceRepositoryFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(ResourceRepositoryMock.Object);
        _streamProviderFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(_streamProviderMock.Object);
    }

    public Mock<ILogger> LoggerMock { get; private set; } = new();
    public Mock<IGraphSearch> GraphSearchMock { get; private set; } = new();
    public Mock<IFactory<Job, ILinkCalculator>> LinkCalculatorFactoryMock { get; } = new();
    public Mock<ILinkCalculator> LinkCalculatorMock { get; private set; } = new();
    public Mock<IAsyncFactory<JobDto, Job>> JobFactoryMock { get; private set; } = new();

    public Mock<IOptionalParameterFactory<Job, IPageMarkerRepository>> PageMarkerRepositoryFactoryMock { get; } =
        new();

    public Mock<IPageMarkerRepository> PageMarkerRepositoryMock { get; } = new();
    public Mock<IFactory<Job, IResourceRepository>> ResourceRepositoryFactoryMock { get; } = new();
    public Mock<IResourceRepository> ResourceRepositoryMock { get; } = new();
    public Mock<IFactory<Job, IPageRetriever>> PageRetrieverFactoryMock { get; } = new();
    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();
    public IEnumerable<IPage> Traversal { get; }

    public JobDto BuildJobDto(ResourceType resourceType, string resourceRepositoryType = "FileSystemRepository")
    {
        var resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>(
            x => x.RepositoryType == resourceRepositoryType);

        return new JobDto(
            null,
            ResourceXPath,
            resourceRepoConfig,
            "https://example.com",
            null,
            null,
            null,
            null,
            resourceType,
            null,
            null);
    }

    public IScrapDownloadsService BuildScrapDownloadsService(JobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new ScrapDownloadsService(
            GraphSearchMock.Object,
            new MockLogger<ScrapDownloadsService>(LoggerMock),
            _streamProviderFactoryMock.Object,
            ResourceRepositoryFactoryMock.Object,
            PageRetrieverFactoryMock.Object,
            PageMarkerRepositoryFactoryMock.Object,
            LinkCalculatorFactoryMock.Object,
            JobFactoryMock.Object);
    }

    public IScrapTextService BuildScrapTextsService(JobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new ScrapTextService(
            GraphSearchMock.Object,
            JobFactoryMock.Object,
            ResourceRepositoryFactoryMock.Object,
            PageRetrieverFactoryMock.Object,
            PageMarkerRepositoryFactoryMock.Object,
            LinkCalculatorFactoryMock.Object,
            new MockLogger<ScrapTextService>(LoggerMock));
    }

    public ITraversalApplicationService BuildTraversalApplicationService(JobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new TraversalApplicationService(
            GraphSearchMock.Object,
            PageRetrieverFactoryMock.Object,
            LinkCalculatorFactoryMock.Object,
            JobFactoryMock.Object);
    }

    public IMarkVisitedApplicationService BuildMarkVisitedApplicationService(JobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new MarkVisitedApplicationService(
            JobFactoryMock.Object,
            PageMarkerRepositoryFactoryMock.Object);
    }

    public IResourcesApplicationService BuildResourcesApplicationService(JobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new ResourcesApplicationService(
            JobFactoryMock.Object,
            PageRetrieverFactoryMock.Object);
    }

    public IDownloadApplicationService BuildDownloadApplicationService(JobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new DownloadApplicationService(
            JobFactoryMock.Object,
            PageRetrieverFactoryMock.Object,
            ResourceRepositoryFactoryMock.Object,
            _streamProviderFactoryMock.Object,
            Mock.Of<ILogger<DownloadApplicationService>>());
    }

    public IDatabaseApplicationService BuildDatabaseApplicationService()
    {
        return new DatabaseApplicationService(PageMarkerRepositoryFactoryMock.Object);
    }

    private void SetupDependencies(JobDto jobDto)
    {
        foreach (var pageMock in Traversal)
        {
            PageRetrieverMock.Setup(x => x.GetPageAsync(pageMock.Uri))
                .ReturnsAsync(pageMock);
        }

        GraphSearchMock = new Mock<IGraphSearch>();

        GraphSearchMock.Setup(x => x.SearchAsync(It.IsAny<Uri>(), It.IsAny<Func<Uri, Task<IPage>>>(),
                It.IsAny<Func<IPage, IAsyncEnumerable<Uri>>>()))
            .Returns(Traversal.ToAsyncEnumerable());

        _streamProviderMock = new Mock<IDownloadStreamProvider>();
        _streamProviderMock.Setup(y => y.GetStreamAsync(It.IsAny<Uri>()))
            .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("some text goes here!..")));
        JobFactoryMock = new Mock<IAsyncFactory<JobDto, Job>>();
        JobFactoryMock.Setup(x => x.Build(It.IsAny<JobDto>()))
            .ReturnsAsync(new Job(jobDto));

        ResourceRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<ResourceInfo>())).ReturnsAsync(false);

        LinkCalculatorMock = new Mock<ILinkCalculator>();
        LoggerMock = new Mock<ILogger>();
        LoggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback((LogLevel _, EventId _, object state, Exception? _, object _) =>
                _output.WriteLine(state.ToString()));
    }

    private class MockLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public MockLogger(Mock<ILogger> loggerMock)
        {
            _logger = loggerMock.Object;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }
    }
}
