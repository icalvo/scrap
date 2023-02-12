﻿using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages;

public enum AsyncPolicyConfiguration
{
    WithCache,
    WithoutCache
}

public class PageRetrieverFactory : IFactory<Job, IPageRetriever>
{
    private readonly IFactory<Job, AsyncPolicyConfiguration, IAsyncPolicy> _asyncPolicyFactory;
    private readonly IFactory<Job, IDownloadStreamProvider> _downloadStreamProviderFactory;
    private readonly ILoggerFactory _loggerFactory;

    public PageRetrieverFactory(
        IFactory<Job, IDownloadStreamProvider> downloadStreamProviderFactory,
        IFactory<Job, AsyncPolicyConfiguration, IAsyncPolicy> asyncPolicyFactory,
        ILoggerFactory loggerFactory)
    {
        _downloadStreamProviderFactory = downloadStreamProviderFactory;
        _asyncPolicyFactory = asyncPolicyFactory;
        _loggerFactory = loggerFactory;
    }

    public IPageRetriever Build(Job job) =>
        new HttpPageRetriever(
            _downloadStreamProviderFactory.Build(job),
            policy: _asyncPolicyFactory.Build(job, AsyncPolicyConfiguration.WithCache),
            noCachePolicy: _asyncPolicyFactory.Build(job, AsyncPolicyConfiguration.WithoutCache),
            _loggerFactory.CreateLogger<HttpPageRetriever>(),
            _loggerFactory);
}
