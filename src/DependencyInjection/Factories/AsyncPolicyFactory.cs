using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Scrap.Domain;
using Scrap.Domain.Jobs;

namespace Scrap.DependencyInjection.Factories;

public class AsyncPolicyFactory : IFactory<Job, IAsyncPolicy>
{
    private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromMinutes(5);
    
    private readonly IAsyncCacheProvider _cacheProvider;
    private readonly ILoggerFactory _loggerFactory;

    public AsyncPolicyFactory(IAsyncCacheProvider asyncCacheProvider, ILoggerFactory loggerFactory)
    {
        _cacheProvider = asyncCacheProvider;
        _loggerFactory = loggerFactory;
    }

    public IAsyncPolicy Build(Job job)
    {
        static bool IsClientError(Exception ex)
            => ex is HttpRequestException { StatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError };

        int httpRequestRetries = job.HttpRequestRetries;
        TimeSpan httpDelay = job.HttpRequestDelayBetweenRetries;
        var cacheLogger = _loggerFactory.CreateLogger("Cache");
        var cachePolicy = Policy.CacheAsync(
            _cacheProvider,
            DefaultCacheTtl,
            (_, key) => { cacheLogger.LogRequest("CACHED", key); },
            (_, _) => { },
            (_, _) => { },
            (_, _, _) => { },
            (_, _, _) => { });
        var retryPolicy = Policy.Handle<Exception>(ex => !IsClientError(ex))
            .WaitAndRetryAsync(
                httpRequestRetries,
                _ => TimeSpan.Zero,
                (exception, _) => { Console.WriteLine(exception.Message); });

        return Policy.WrapAsync(cachePolicy, retryPolicy, AsyncDelayPolicy.Create(httpDelay));
    }
    
}
