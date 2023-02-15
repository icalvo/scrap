using Moq;
using Scrap.Application;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Pages;

namespace Scrap.Tests.Unit.ApplicationServices;

public class TraversalApplicationServiceMockBuilder
{
    public const string ResourceXPath = "//img/@src";
    private readonly Mock<IFactory<Job, ILinkCalculator>> _linkCalculatorFactoryMock = new();
    private readonly Mock<IFactory<Job, IPageRetriever>> _pageRetrieverFactoryMock = new();

    public TraversalApplicationServiceMockBuilder()
    {
        _linkCalculatorFactoryMock.SetupFactory(LinkCalculatorMock.Object);
        _pageRetrieverFactoryMock.SetupFactory(PageRetrieverMock.Object);
    }

    public Mock<IAsyncFactory<JobDto, Job>> JobFactoryMock { get; } = new();
    public Mock<IGraphSearch> GraphSearchMock { get; private set; } = new();
    public Mock<ILinkCalculator> LinkCalculatorMock { get; } = new();
    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();

    public ITraversalApplicationService Build() =>
        new TraversalApplicationService(
            GraphSearchMock.Object,
            _pageRetrieverFactoryMock.Object,
            _linkCalculatorFactoryMock.Object,
            JobFactoryMock.Object);

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
