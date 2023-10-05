using Microsoft.Extensions.Logging;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Infrastructure.Factories;

public class PageRetrieverFactory : IPageRetrieverFactory
{
    private readonly IAsyncPolicyFactory _asyncPolicyFactory;
    private readonly IDownloadStreamProviderFactory _downloadStreamProviderFactory;
    private readonly ILoggerFactory _loggerFactory;

    public PageRetrieverFactory(
        IDownloadStreamProviderFactory downloadStreamProviderFactory,
        IAsyncPolicyFactory asyncPolicyFactory,
        ILoggerFactory loggerFactory)
    {
        _downloadStreamProviderFactory = downloadStreamProviderFactory;
        _asyncPolicyFactory = asyncPolicyFactory;
        _loggerFactory = loggerFactory;
    }

    public IPageRetriever Build(IPageRetrieverOptions options) =>
        new HttpPageRetriever(
            _downloadStreamProviderFactory.Build(options),
            _asyncPolicyFactory.Build(options, AsyncPolicyConfiguration.WithCache),
            _asyncPolicyFactory.Build(options, AsyncPolicyConfiguration.WithoutCache),
            _loggerFactory.CreateLogger<HttpPageRetriever>(),
            _loggerFactory);
}
