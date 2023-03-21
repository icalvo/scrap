using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Application.Scrap;
using Scrap.Common.Graphs;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class ScrapTextsServiceMockBuilder
{
    public const string ResourceXPath = "//img/@src";
    private readonly Mock<ILinkCalculatorFactory> _linkCalculatorFactoryMock = new();
    private readonly ITestOutputHelper _output;

    private readonly Mock<IPageMarkerRepositoryFactory> _pageMarkerRepositoryFactoryMock =
        new();

    private readonly Mock<IPageRetrieverFactory> _pageRetrieverFactoryMock = new();
    private readonly Mock<IResourceRepositoryFactory> _resourceRepositoryFactoryMock = new();

    public ScrapTextsServiceMockBuilder(ITestOutputHelper output)
    {
        _output = output;
        _linkCalculatorFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(LinkCalculatorMock.Object);
        _pageMarkerRepositoryFactoryMock.Setup(x => x.Build()).Returns(PageMarkerRepositoryMock.Object);
        _pageMarkerRepositoryFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(PageMarkerRepositoryMock.Object);
        _pageRetrieverFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(PageRetrieverMock.Object);
        _resourceRepositoryFactoryMock.Setup(x => x.BuildAsync(It.IsAny<Job>())).ReturnsAsync(ResourceRepositoryMock.Object);
        LoggerMock.SetupWithOutput(output);
    }

    public Mock<IJobFactory> JobFactoryMock { get; } = new();
    public Mock<ILogger> LoggerMock { get; } = new();
    public Mock<IGraphSearch> GraphSearchMock { get; private set; } = new();
    public Mock<ILinkCalculator> LinkCalculatorMock { get; } = new();
    public Mock<IPageMarkerRepository> PageMarkerRepositoryMock { get; } = new();
    public Mock<IResourceRepository> ResourceRepositoryMock { get; } = new();
    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();

    public IScrapTextService Build() =>
        new ScrapTextService(
            GraphSearchMock.Object,
            JobFactoryMock.Object,
            _resourceRepositoryFactoryMock.Object,
            _pageRetrieverFactoryMock.Object,
            _pageMarkerRepositoryFactoryMock.Object,
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
