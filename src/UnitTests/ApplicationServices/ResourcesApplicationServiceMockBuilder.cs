using Moq;
using Scrap.Application;
using Scrap.Application.Download;
using Scrap.Application.Resources;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Tests.Unit.ApplicationServices;

public class ResourcesApplicationServiceMockBuilder
{
    private readonly Mock<IPageRetrieverFactory> _pageRetrieverFactoryMock = new();

    public ResourcesApplicationServiceMockBuilder()
    {
        _pageRetrieverFactoryMock.Setup(x => x.Build(It.IsAny<IPageRetrieverOptions>())).Returns(PageRetrieverMock.Object);
    }

    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();
    public Mock<ICommandJobBuilder<IResourceCommand, IResourcesJob>> CommandJobBuilderMock { get; } = new();

    public IResourcesApplicationService Build() =>
        new ResourcesApplicationService(_pageRetrieverFactoryMock.Object, CommandJobBuilderMock.Object);

    public void SetupTraversal(params IPage[] pages)
    {
        foreach (var pageMock in pages)
        {
            PageRetrieverMock.Setup(x => x.GetPageAsync(pageMock.Uri)).ReturnsAsync(pageMock);
        }
    }
}
