using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Infrastructure.Resilience;

namespace Scrap.Infrastructure.Factories;

public class AsyncPolicyFactory : IAsyncPolicyFactory
{
    private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromMinutes(5);

    private readonly IMemoryCache _memoryCache;
    private readonly ILoggerFactory _loggerFactory;

    public AsyncPolicyFactory(IMemoryCache memoryCache, ILoggerFactory loggerFactory)
    {
        _memoryCache = memoryCache;
        _loggerFactory = loggerFactory;
    }

    public ResiliencePipeline Build(Job job, AsyncPolicyConfiguration config)
    {
        var builder = new ResiliencePipelineBuilder();

        if (config == AsyncPolicyConfiguration.WithCache)
        {
            var cacheLogger = _loggerFactory.CreateLogger("Cache");
            builder.AddStrategy(
                _ => new MemoryCacheResilienceStrategy(_memoryCache, DefaultCacheTtl, cacheLogger),
                new MemoryCacheStrategyOptions());
        }

        if (job.HttpRequestRetries > 0)
        {
            builder.AddRetry(BuildRetryOptions(job.HttpRequestRetries));
        }

        if (job.HttpRequestDelayBetweenRetries > TimeSpan.Zero)
        {
            builder.AddStrategy(
                _ => new FixedDelayResilienceStrategy(job.HttpRequestDelayBetweenRetries),
                new FixedDelayStrategyOptions());
        }

        return builder.Build();
    }

    private static RetryStrategyOptions BuildRetryOptions(int httpRequestRetries) =>
        new()
        {
            MaxRetryAttempts = httpRequestRetries,
            Delay = TimeSpan.Zero,
            ShouldHandle = static args =>
            {
                if (args.Outcome.Exception is HttpRequestException
                    {
                        StatusCode: >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError
                    })
                {
                    return PredicateResult.False();
                }

                return args.Outcome.Exception is not null
                    ? PredicateResult.True()
                    : PredicateResult.False();
            },
            OnRetry = static args =>
            {
                Console.WriteLine(args.Outcome.Exception?.Message);
                return ValueTask.CompletedTask;
            }
        };
}
