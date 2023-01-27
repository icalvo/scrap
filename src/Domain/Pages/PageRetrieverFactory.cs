using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public class PageRetrieverFactory : IFactory<Job, IPageRetriever>
{
    private readonly IFactory<Job, IDownloadStreamProvider> _downloadStreamProviderFactory;
    private readonly IFactory<Job, IAsyncPolicy> _asyncPolicyFactory;
    private readonly ILoggerFactory _loggerFactory;

    public PageRetrieverFactory(
        IFactory<Job, IDownloadStreamProvider> downloadStreamProviderFactory,
        IFactory<Job, IAsyncPolicy> asyncPolicyFactory,
        ILoggerFactory loggerFactory)
    {
        _downloadStreamProviderFactory = downloadStreamProviderFactory;
        _asyncPolicyFactory = asyncPolicyFactory;
        _loggerFactory = loggerFactory;
    }

    public IPageRetriever Build(Job job)
    {
        return new HttpPageRetriever(_downloadStreamProviderFactory.Build(job),
            _asyncPolicyFactory.Build(job),
            _loggerFactory.CreateLogger<HttpPageRetriever>(),
            _loggerFactory);
    }
}
