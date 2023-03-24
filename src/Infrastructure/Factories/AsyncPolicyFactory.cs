﻿using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Infrastructure.Factories;

public class AsyncPolicyFactory : IAsyncPolicyFactory
{
    private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromMinutes(5);

    private readonly IAsyncCacheProvider _cacheProvider;
    private readonly ILoggerFactory _loggerFactory;

    public AsyncPolicyFactory(IAsyncCacheProvider asyncCacheProvider, ILoggerFactory loggerFactory)
    {
        _cacheProvider = asyncCacheProvider;
        _loggerFactory = loggerFactory;
    }

    public IAsyncPolicy Build(Job job, AsyncPolicyConfiguration config)
    {
        static bool IsClientError(Exception ex) =>
            ex is HttpRequestException
            {
                StatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError
            };

        var httpRequestRetries = job.HttpRequestRetries;
        var httpDelay = job.HttpRequestDelayBetweenRetries;
        var retryPolicy = Policy.Handle<Exception>(ex => !IsClientError(ex)).WaitAndRetryAsync(
            httpRequestRetries,
            _ => TimeSpan.Zero,
            (exception, _) => { Console.WriteLine(exception.Message); });


        if (config == AsyncPolicyConfiguration.WithoutCache)
        {
            return Policy.WrapAsync(retryPolicy, AsyncDelayPolicy.Create(httpDelay));
        }

        var cacheLogger = _loggerFactory.CreateLogger("Cache");
        var cachePolicy = Policy.CacheAsync(
            _cacheProvider,
            DefaultCacheTtl,
            (_, key) => { cacheLogger.LogRequest("CACHED", key); },
            (_, _) => { },
            (_, _) => { },
            (_, _, _) => { },
            (_, _, _) => { });
        return Policy.WrapAsync(cachePolicy, retryPolicy, AsyncDelayPolicy.Create(httpDelay));
    }
}