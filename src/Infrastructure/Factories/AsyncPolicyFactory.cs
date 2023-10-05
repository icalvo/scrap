using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Polly.Retry;
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

    public IAsyncPolicy Build(IAsyncPolicyOptions options, AsyncPolicyConfiguration config) =>
        Policy.WrapAsync(Policies(options, config).ToArray());

    private IEnumerable<IAsyncPolicy> Policies(IAsyncPolicyOptions job, AsyncPolicyConfiguration config)
    {
        if (config == AsyncPolicyConfiguration.WithCache)
        {
            yield return BuildCachePolicy();
        }

        yield return BuildRetryPolicy(job.HttpRequestRetries);
        yield return AsyncDelayPolicy.Create(job.HttpRequestDelayBetweenRetries);
    }

    private static AsyncRetryPolicy BuildRetryPolicy(int httpRequestRetries)
    {
        static bool IsClientError(Exception ex) =>
            ex is HttpRequestException
            {
                StatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError
            };

        return Policy.Handle<Exception>(ex => !IsClientError(ex)).WaitAndRetryAsync(
            httpRequestRetries,
            _ => TimeSpan.Zero,
            (exception, _) => { Console.WriteLine(exception.Message); });
    }

    private AsyncCachePolicy BuildCachePolicy()
    {
        var cacheLogger = _loggerFactory.CreateLogger("Cache");
        return Policy.CacheAsync(
            _cacheProvider,
            DefaultCacheTtl,
            (_, key) => { cacheLogger.LogRequest("CACHED", key); },
            (_, _) => { },
            (_, _) => { },
            (_, _, _) => { },
            (_, _, _) => { });
    }
}
