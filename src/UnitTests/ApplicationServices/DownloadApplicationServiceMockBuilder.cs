using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Application;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class DownloadApplicationServiceMockBuilder
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<IFactory<Job, IPageRetriever>> _pageRetrieverFactoryMock = new();
    private readonly Mock<IFactory<Job, IResourceRepository>> _resourceRepositoryFactoryMock = new();
    private readonly Mock<IFactory<Job, IDownloadStreamProvider>> _streamProviderFactoryMock = new();
    private readonly Mock<IDownloadStreamProvider> _streamProviderMock = new();

    public DownloadApplicationServiceMockBuilder(ITestOutputHelper output)
    {
        _output = output;
        _pageRetrieverFactoryMock.SetupFactory(PageRetrieverMock.Object);
        _resourceRepositoryFactoryMock.SetupFactory(ResourceRepositoryMock.Object);
        _streamProviderFactoryMock.SetupFactory(_streamProviderMock.Object);
        _streamProviderMock.SetupWithString();
        LoggerMock.SetupWithOutput(output);
    }

    public Mock<IAsyncFactory<JobDto, Job>> JobFactoryMock { get; } = new();
    public Mock<ILogger> LoggerMock { get; } = new();
    public Mock<IResourceRepository> ResourceRepositoryMock { get; } = new();
    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();

    public IDownloadApplicationService Build() =>
        new DownloadApplicationService(
            JobFactoryMock.Object,
            _pageRetrieverFactoryMock.Object,
            _resourceRepositoryFactoryMock.Object,
            _streamProviderFactoryMock.Object,
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
