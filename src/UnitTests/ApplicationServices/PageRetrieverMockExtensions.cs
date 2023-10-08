using Moq;
using Scrap.Domain.Pages;

namespace Scrap.Tests.Unit.ApplicationServices;

public static class PageRetrieverMockExtensions
{
    
    public static void SetupTraversal(this Mock<IPageRetriever> pageRetrieverMock, params IPage[] pages)
    {
        foreach (var pageMock in pages)
        {
            pageRetrieverMock.Setup(x => x.GetPageAsync(pageMock.Uri)).ReturnsAsync(pageMock);
        }
    }

    public static void SetupTraversal2(this Mock<IPageRetriever> pageRetrieverMock, params IPage[] pages)
    {
        foreach (var pageMock in pages)
        {
            pageRetrieverMock.Setup(x => x.GetPageAsync(pageMock.Uri)).ThrowsAsync(new Exception());
            pageRetrieverMock.Setup(x => x.GetPageWithoutCacheAsync(pageMock.Uri)).ReturnsAsync(pageMock);
        }
    }
}
