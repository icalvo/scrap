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

public class ScrapDownloadsServiceMockBuilder
{
    public const string ResourceXPath = "//img/@src";
    private readonly Mock<IFactory<Job, ILinkCalculator>> _linkCalculatorFactoryMock = new();

    private readonly Mock<IOptionalParameterFactory<Job, IPageMarkerRepository>> _pageMarkerRepositoryFactoryMock =
        new();

    private readonly Mock<IFactory<Job, IPageRetriever>> _pageRetrieverFactoryMock = new();
    private readonly Mock<IFactory<Job, IResourceRepository>> _resourceRepositoryFactoryMock = new();

    private readonly Mock<IFactory<Job, IDownloadStreamProvider>> _streamProviderFactoryMock = new();
    private readonly Mock<IDownloadStreamProvider> _streamProviderMock = new();

    public ScrapDownloadsServiceMockBuilder(ITestOutputHelper output)
    {
        _linkCalculatorFactoryMock.SetupFactory(LinkCalculatorMock.Object);
        _pageMarkerRepositoryFactoryMock.SetupFactory(PageMarkerRepositoryMock.Object);
        _pageRetrieverFactoryMock.SetupFactory(PageRetrieverMock.Object);
        _resourceRepositoryFactoryMock.SetupFactory(ResourceRepositoryMock.Object);
        _streamProviderFactoryMock.SetupFactory(_streamProviderMock.Object);

        _streamProviderMock.SetupWithString();

        ResourceRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<ResourceInfo>())).ReturnsAsync(false);
        LoggerMock.SetupWithOutput(output);
    }


    public Mock<IAsyncFactory<JobDto, Job>> JobFactoryMock { get; } = new();
    public Mock<ILogger> LoggerMock { get; set; } = new();
    public Mock<IGraphSearch> GraphSearchMock { get; private set; } = new();
    public Mock<ILinkCalculator> LinkCalculatorMock { get; set; } = new();
    public Mock<IPageMarkerRepository> PageMarkerRepositoryMock { get; } = new();
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
            _pageMarkerRepositoryFactoryMock.Object,
            _linkCalculatorFactoryMock.Object,
            JobFactoryMock.Object);
}
