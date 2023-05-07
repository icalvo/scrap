using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Common.Graphs;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit;

public class ScrapDownloadsServiceMockBuilder
{
    public const string ResourceXPath = "//img/@src";
    private readonly Mock<ILinkCalculatorFactory> _linkCalculatorFactoryMock = new();

    private readonly Mock<IVisitedPageRepositoryFactory> _visitedPageRepositoryFactoryMock =
        new();

    private readonly Mock<IPageRetrieverFactory> _pageRetrieverFactoryMock = new();
    private readonly Mock<IResourceRepositoryFactory> _resourceRepositoryFactoryMock = new();

    private readonly Mock<IDownloadStreamProviderFactory> _streamProviderFactoryMock = new();
    private readonly Mock<IDownloadStreamProvider> _streamProviderMock = new();

    public ScrapDownloadsServiceMockBuilder(ITestOutputHelper output)
    {
        _linkCalculatorFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(LinkCalculatorMock.Object);
        _visitedPageRepositoryFactoryMock.Setup(x => x.Build()).Returns(VisitedPageRepositoryMock.Object);
        _visitedPageRepositoryFactoryMock.Setup(x => x.Build(It.IsAny<Job>()))
            .Returns(VisitedPageRepositoryMock.Object);
        _pageRetrieverFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(PageRetrieverMock.Object);
        _resourceRepositoryFactoryMock.Setup(x => x.BuildAsync(It.IsAny<Job>())).ReturnsAsync(ResourceRepositoryMock.Object);
        _streamProviderFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(_streamProviderMock.Object);

        _streamProviderMock.SetupWithString();

        ResourceRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<ResourceInfo>())).ReturnsAsync(false);
        LoggerMock.SetupWithOutput(output);
    }

    public Mock<ILogger> LoggerMock { get; set; } = new();
    public Mock<IGraphSearch> GraphSearchMock { get; private set; } = new();
    public Mock<ILinkCalculator> LinkCalculatorMock { get; set; } = new();
    public Mock<IVisitedPageRepository> VisitedPageRepositoryMock { get; } = new();
    public Mock<IResourceRepository> ResourceRepositoryMock { get; } = new();
    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();

    public void SetupTraversal(params IPage[] pages)
    {
        foreach (var pageMock in pages)
        {
            PageRetrieverMock.Setup(x => x.GetPageAsync(pageMock.Uri)).ReturnsAsync(pageMock);
        }

        GraphSearchMock = new Mock<IGraphSearch>();

        GraphSearchMock
            .Setup(
                x => x.SearchAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<Func<Uri, Task<IPage>>>(),
                    It.IsAny<Func<IPage, IAsyncEnumerable<Uri>>>())).Returns(pages.ToAsyncEnumerable());
    }

    public void SetupTraversal2(params IPage[] pages)
    {
        foreach (var pageMock in pages)
        {
            PageRetrieverMock.Setup(x => x.GetPageAsync(pageMock.Uri)).ThrowsAsync(new Exception());
            PageRetrieverMock.Setup(x => x.GetPageWithoutCacheAsync(pageMock.Uri)).ReturnsAsync(pageMock);
        }

        GraphSearchMock = new Mock<IGraphSearch>();

        GraphSearchMock
            .Setup(
                x => x.SearchAsync(
                    It.IsAny<Uri>(),
                    It.IsAny<Func<Uri, Task<IPage>>>(),
                    It.IsAny<Func<IPage, IAsyncEnumerable<Uri>>>())).Returns(pages.ToAsyncEnumerable());
    }

    public IScrapDownloadsService BuildScrapDownloadsService() =>
        new ScrapDownloadsService(
            GraphSearchMock.Object,
            LoggerMock.Object.ToGeneric<ScrapDownloadsService>(),
            _streamProviderFactoryMock.Object,
            _resourceRepositoryFactoryMock.Object,
            _pageRetrieverFactoryMock.Object,
            _visitedPageRepositoryFactoryMock.Object,
            _linkCalculatorFactoryMock.Object);
}
