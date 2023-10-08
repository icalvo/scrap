using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Common.Graphs;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit;

public class ScrapTextsServiceMockBuilder
{
    private readonly Mock<ILinkCalculatorFactory> _linkCalculatorFactoryMock = new();

    private readonly Mock<IVisitedPageRepositoryFactory> _visitedPageRepositoryFactoryMock =
        new();

    private readonly Mock<IPageRetrieverFactory> _pageRetrieverFactoryMock = new();
    private readonly Mock<IResourceRepositoryFactory> _resourceRepositoryFactoryMock = new();

    public ScrapTextsServiceMockBuilder(ITestOutputHelper output)
    {
        _linkCalculatorFactoryMock.Setup(x => x.Build(It.IsAny<ILinkCalculatorOptions>())).Returns(LinkCalculatorMock.Object);
        _visitedPageRepositoryFactoryMock.Setup(x => x.Build()).Returns(VisitedPageRepositoryMock.Object);
        _visitedPageRepositoryFactoryMock.Setup(x => x.Build(It.IsAny<IVisitedPageRepositoryOptions>()))
            .Returns(VisitedPageRepositoryMock.Object);
        _pageRetrieverFactoryMock.Setup(x => x.Build(It.IsAny<IPageRetrieverOptions>())).Returns(PageRetrieverMock.Object);
        _resourceRepositoryFactoryMock.Setup(x => x.BuildAsync(It.IsAny<IResourceRepositoryOptions>())).ReturnsAsync(ResourceRepositoryMock.Object);
        LoggerMock.SetupWithOutput(output);
    }

    public Mock<ILogger> LoggerMock { get; } = new();
    public Mock<IGraphSearch> GraphSearchMock { get; private set; } = new();
    public Mock<ILinkCalculator> LinkCalculatorMock { get; } = new();
    public Mock<IVisitedPageRepository> VisitedPageRepositoryMock { get; } = new();
    public Mock<IResourceRepository> ResourceRepositoryMock { get; } = new();
    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();

    public IScrapTextService Build() =>
        new ScrapTextService(
            GraphSearchMock.Object,
            _resourceRepositoryFactoryMock.Object,
            _pageRetrieverFactoryMock.Object,
            _visitedPageRepositoryFactoryMock.Object,
            _linkCalculatorFactoryMock.Object,
            LoggerMock.Object.ToGeneric<ScrapTextService>());

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
}
