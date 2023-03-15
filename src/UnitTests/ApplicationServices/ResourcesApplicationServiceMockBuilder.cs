using Moq;
using Scrap.Application;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Tests.Unit.ApplicationServices;

public class ResourcesApplicationServiceMockBuilder
{
    private readonly Mock<IPageRetrieverFactory> _pageRetrieverFactoryMock = new();

    public ResourcesApplicationServiceMockBuilder()
    {
        _pageRetrieverFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(PageRetrieverMock.Object);
        _pageRetrieverFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(PageRetrieverMock.Object);
    }

    public Mock<IJobFactory> JobFactoryMock { get; } = new();
    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();

    public IResourcesApplicationService Build() =>
        new ResourcesApplicationService(JobFactoryMock.Object, _pageRetrieverFactoryMock.Object);

    public void SetupTraversal(params IPage[] pages)
    {
        foreach (var pageMock in pages)
        {
            PageRetrieverMock.Setup(x => x.GetPageAsync(pageMock.Uri)).ReturnsAsync(pageMock);
        }
    }

    public void SetupTraversal2(params IPage[] pages)
    {
        foreach (var pageMock in pages)
        {
            PageRetrieverMock.Setup(x => x.GetPageAsync(pageMock.Uri)).ThrowsAsync(new Exception());
            PageRetrieverMock.Setup(x => x.GetPageWithoutCacheAsync(pageMock.Uri)).ReturnsAsync(pageMock);
        }
    }
}
