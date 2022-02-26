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
    private Mock<IDownloadStreamProvider> _streamProviderMock = new();
    public Mock<ILogger> LoggerMock { get; private set; } = new();
    public Mock<IGraphSearch> GraphSearchMock { get; private set; } = new();
    public Mock<ILinkCalculator> LinkCalculatorMock { get; private set; } = new();
    public Mock<IJobFactory> JobFactoryMock { get; private set; } = new();
    public Mock<IPageMarkerRepository> PageMarkerRepositoryMock { get; } = new();
    public Mock<IResourceRepository> ResourceRepositoryMock { get; } = new();
    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();
    public IEnumerable<IPage> Traversal { get; }

    public MockBuilder(ITestOutputHelper output, params IPage[] traversal)
    {
        _output = output;
        Traversal = traversal;
    }

    public NewJobDto BuildJobDto(ResourceType resourceType = ResourceType.DownloadLink)
    {
        IResourceRepositoryConfiguration resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>();

        return new NewJobDto(
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
        
    public ScrapDownloadsService BuildScrapDownloadsService(NewJobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new ScrapDownloadsService(
            GraphSearchMock.Object,
            new MockLogger<ScrapDownloadsService>(LoggerMock),
            _streamProviderMock.Object,
            ResourceRepositoryMock.Object,
            PageRetrieverMock.Object,
            PageMarkerRepositoryMock.Object,
            LinkCalculatorMock.Object,
            JobFactoryMock.Object);
    }

    public IScrapTextService BuildScrapTextsService(NewJobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new ScrapTextService(
            GraphSearchMock.Object,
            JobFactoryMock.Object,
            ResourceRepositoryMock.Object,
            PageRetrieverMock.Object,
            PageMarkerRepositoryMock.Object,
            LinkCalculatorMock.Object,
            new MockLogger<ScrapTextService>(LoggerMock));
    }

    public TraversalApplicationService BuildTraversalApplicationService(NewJobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new TraversalApplicationService(GraphSearchMock.Object,
            PageRetrieverMock.Object,
            LinkCalculatorMock.Object,
            JobFactoryMock.Object);
    }

    public MarkVisitedApplicationService BuildMarkVisitedApplicationService(NewJobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new MarkVisitedApplicationService(
            JobFactoryMock.Object,
            PageMarkerRepositoryMock.Object);
    }

    public ResourcesApplicationService BuildResourcesApplicationService(NewJobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new ResourcesApplicationService(
            JobFactoryMock.Object,
            PageRetrieverMock.Object);
    }

    public DownloadApplicationService BuildDownloadApplicationService(NewJobDto jobDto)
    {
        SetupDependencies(jobDto);
        return new DownloadApplicationService(
            JobFactoryMock.Object,
            PageRetrieverMock.Object,
            ResourceRepositoryMock.Object,
            _streamProviderMock.Object,
            Mock.Of<ILogger<DownloadApplicationService>>());
    }

    private void SetupDependencies(NewJobDto jobDto)
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
        JobFactoryMock = new Mock<IJobFactory>();
        JobFactoryMock.Setup(x => x.CreateAsync(It.IsAny<NewJobDto>()))
            .ReturnsAsync(new Job(jobDto));

        ResourceRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<ResourceInfo>())).ReturnsAsync(false);

        LinkCalculatorMock = new Mock<ILinkCalculator>();
        LoggerMock = new Mock<ILogger>();
        LoggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback((LogLevel _, EventId _, object state, Exception? _, object _) => _output.WriteLine(state.ToString()));
    }

    private class MockLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public MockLogger(Mock<ILogger> loggerMock)
        {
            _logger = loggerMock.Object;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => _logger.Log(logLevel, eventId, state, exception, formatter);

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
    }
}
