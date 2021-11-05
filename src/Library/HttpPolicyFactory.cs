using System;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Caching;
using Scrap.DependencyInjection;

namespace Scrap
{
    public class HttpPolicyFactory
    {
        private const int DefaultHttpRequestRetries = 5;
        private static readonly TimeSpan DefaultHttpRequestDelayBetweenRetries = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan DefaultCacheTtl = TimeSpan.FromMinutes(5);
        private readonly ILoggerFactory _loggerFactory;
        private readonly IAsyncCacheProvider _cacheProvider;

        public HttpPolicyFactory(ILoggerFactory loggerFactory, IAsyncCacheProvider cacheProvider)
        {
            _loggerFactory = loggerFactory;
            _cacheProvider = cacheProvider;
        }

        public IAsyncPolicy Build(
            int? httpRequestRetries,
            TimeSpan? httpDelay)
        {
            var cacheLogger = _loggerFactory.CreateLogger("Cache");
            var cachePolicy = Policy.CacheAsync(
                _cacheProvider,
                DefaultCacheTtl,
                (_, key) => { cacheLogger.LogInformation("CACHED {Uri}", key); },
                (_, _) => {  },
                (_, _) => {  },
                (_, _, _) => {  },
                (_, _, _) => {  });
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    httpRequestRetries ?? DefaultHttpRequestRetries,
                    _ => TimeSpan.Zero,
                    (exception, _) =>
                    {
                        Console.WriteLine(exception.Message);
                    });

            return Policy.WrapAsync(
                cachePolicy,
                retryPolicy,
                new AsyncDelayPolicy(httpDelay ?? DefaultHttpRequestDelayBetweenRetries));

        }        
    }
}