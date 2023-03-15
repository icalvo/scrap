using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Common;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

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

    public IPageRetriever Build(Job job) =>
        new HttpPageRetriever(
            _downloadStreamProviderFactory.Build(job),
            _asyncPolicyFactory.Build(job, AsyncPolicyConfiguration.WithCache),
            _asyncPolicyFactory.Build(job, AsyncPolicyConfiguration.WithoutCache),
            _loggerFactory.CreateLogger<HttpPageRetriever>(),
            _loggerFactory);
}
