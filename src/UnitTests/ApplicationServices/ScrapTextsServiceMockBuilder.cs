using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Application.Scrap;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class ScrapTextsServiceMockBuilder
{
    public const string ResourceXPath = "//img/@src";
    private readonly Mock<IFactory<Job, ILinkCalculator>> _linkCalculatorFactoryMock = new();
    private readonly ITestOutputHelper _output;

    private readonly Mock<IOptionalParameterFactory<Job, IPageMarkerRepository>> _pageMarkerRepositoryFactoryMock =
        new();

    private readonly Mock<IFactory<Job, IPageRetriever>> _pageRetrieverFactoryMock = new();
    private readonly Mock<IFactory<Job, IResourceRepository>> _resourceRepositoryFactoryMock = new();
    private readonly Mock<IFactory<Job, IDownloadStreamProvider>> _streamProviderFactoryMock = new();

    public ScrapTextsServiceMockBuilder(ITestOutputHelper output)
    {
        _output = output;
        _linkCalculatorFactoryMock.SetupFactory(LinkCalculatorMock.Object);
        _pageMarkerRepositoryFactoryMock.SetupFactory(PageMarkerRepositoryMock.Object);
        _pageRetrieverFactoryMock.SetupFactory(PageRetrieverMock.Object);
        _resourceRepositoryFactoryMock.SetupFactory(ResourceRepositoryMock.Object);
        LoggerMock.SetupWithOutput(output);
    }

    public Mock<IAsyncFactory<JobDto, Job>> JobFactoryMock { get; } = new();
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
