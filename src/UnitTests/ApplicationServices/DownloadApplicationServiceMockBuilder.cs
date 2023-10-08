using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using Scrap.Application;
using Scrap.Application.Download;
using Scrap.Common;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;
using Scrap.Domain.Sites;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class DownloadApplicationServiceMockBuilder
{
    private readonly IPageRetrieverFactory _pageRetrieverFactory = Substitute.For<IPageRetrieverFactory>();

    private readonly IResourceRepositoryFactory _resourceRepositoryFactory =
        Substitute.For<IResourceRepositoryFactory>();

    private readonly IDownloadStreamProviderFactory _streamProviderFactory =
        Substitute.For<IDownloadStreamProviderFactory>();

    private readonly IDownloadStreamProvider _streamProvider = Substitute.For<IDownloadStreamProvider>();

    public DownloadApplicationServiceMockBuilder(ITestOutputHelper output)
    {
        _pageRetrieverFactory.Build(Arg.Any<IPageRetrieverOptions>()).Returns(PageRetrieverMock.Object);
        _resourceRepositoryFactory.BuildAsync(Arg.Any<IResourceRepositoryOptions>()).Returns(ResourceRepositoryMock.Object);
        _streamProviderFactory.Build(Arg.Any<IDownloadStreamProviderOptions>()).Returns(_streamProvider);
        _streamProvider.SetupWithString();
        LoggerMock.SetupWithOutput(output);
    }

    public Mock<ILogger> LoggerMock { get; } = new();
    public Mock<IResourceRepository> ResourceRepositoryMock { get; } = new();
    public Mock<IPageRetriever> PageRetrieverMock { get; } = new();
    public Mock<ICommandJobBuilder<IDownloadCommand, IDownloadJob>> CommandJobBuilderMock { get; } = new();

    public IDownloadApplicationService Build() =>
        new DownloadApplicationService(
            _pageRetrieverFactory,
            _resourceRepositoryFactory,
            _streamProviderFactory,
            LoggerMock.Object.ToGeneric<DownloadApplicationService>(),
            CommandJobBuilderMock.Object);
}

public static class CommandJobBuilderExtensions
{
    public static void SetupCommandJobBuilder<TCommand, TJob>(
        this Mock<ICommandJobBuilder<TCommand, TJob>> commandJobBuilderMock, TJob job, string siteName)
    {
        commandJobBuilderMock.Setup(x =>
            x.Build(It.IsAny<TCommand>())).ReturnsAsync(
            (job, new Site(siteName)).ToUnitResult());
    }
}
