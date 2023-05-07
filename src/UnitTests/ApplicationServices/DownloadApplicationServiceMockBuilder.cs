using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Application.Download;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Scrap.Domain.Sites;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class DownloadApplicationServiceMockBuilder
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<IPageRetrieverFactory> _pageRetrieverFactoryMock = new();
    private readonly Mock<IResourceRepositoryFactory> _resourceRepositoryFactoryMock = new();
    private readonly Mock<IDownloadStreamProviderFactory> _streamProviderFactoryMock = new();
    private readonly Mock<IDownloadStreamProvider> _streamProviderMock = new();

    public DownloadApplicationServiceMockBuilder(ITestOutputHelper output)
    {
        _output = output;

        _pageRetrieverFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(PageRetrieverMock.Object);
        _resourceRepositoryFactoryMock.Setup(x => x.BuildAsync(It.IsAny<Job>())).ReturnsAsync(ResourceRepositoryMock.Object);
        _streamProviderFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(_streamProviderMock.Object);
        _streamProviderMock.SetupWithString();
        LoggerMock.SetupWithOutput(output);
    }

    public Mock<ILogger> LoggerMock { get; } = new();
    public Mock<IResourceRepository> ResourceRepositoryMock { get; } = new();
    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();
    public Mock<ISiteService> SiteServiceMock { get; } = new();

    public IDownloadApplicationService Build() =>
        new DownloadApplicationService(
            _pageRetrieverFactoryMock.Object,
            _resourceRepositoryFactoryMock.Object,
            _streamProviderFactoryMock.Object,
            SiteServiceMock.Object,
            LoggerMock.Object.ToGeneric<DownloadApplicationService>());

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
