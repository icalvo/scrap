using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using Scrap.Application.Download;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class DownloadApplicationServiceMockBuilder
{
    private readonly ITestOutputHelper _output;
    private readonly IPageRetrieverFactory _pageRetrieverFactory = Substitute.For<IPageRetrieverFactory>();

    private readonly IResourceRepositoryFactory _resourceRepositoryFactory =
        Substitute.For<IResourceRepositoryFactory>();

    private readonly IDownloadStreamProviderFactory _streamProviderFactory =
        Substitute.For<IDownloadStreamProviderFactory>();

    private readonly IDownloadStreamProvider _streamProvider = Substitute.For<IDownloadStreamProvider>();

    public DownloadApplicationServiceMockBuilder(ITestOutputHelper output)
    {
        _output = output;

        _pageRetrieverFactory.Build(Arg.Any<Job>()).Returns(PageRetrieverMock.Object);
        _resourceRepositoryFactory.BuildAsync(Arg.Any<Job>()).Returns(ResourceRepositoryMock.Object);
        _streamProviderFactory.Build(Arg.Any<Job>()).Returns(_streamProvider);
        _streamProvider.SetupWithString();
        LoggerMock.SetupWithOutput(output);
    }

    public Mock<ILogger> LoggerMock { get; } = new();
    public Mock<IResourceRepository> ResourceRepositoryMock { get; } = new();
    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();
    public Mock<IJobService> JobServiceMock { get; } = new();

    public IDownloadApplicationService Build() =>
        new DownloadApplicationService(
            _pageRetrieverFactory,
            _resourceRepositoryFactory,
            _streamProviderFactory,
            JobServiceMock.Object,
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
