using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Infrastructure;

namespace Scrap.Infrastructure.Resilience;

internal sealed class MemoryCacheResilienceStrategy : ResilienceStrategy
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _ttl;
    private readonly ILogger _logger;

    public MemoryCacheResilienceStrategy(IMemoryCache cache, TimeSpan ttl, ILogger logger)
    {
        _cache = cache;
        _ttl = ttl;
        _logger = logger;
    }

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        var key = context.OperationKey;
        if (!string.IsNullOrEmpty(key) && _cache.TryGetValue(key, out TResult? cached) && cached is not null)
        {
            _logger.LogRequest("CACHED", key);
            return Outcome.FromResult(cached);
        }

        var outcome = await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);

        if (!string.IsNullOrEmpty(key) && outcome is { Exception: null, Result: not null })
        {
            _cache.Set(key, outcome.Result, _ttl);
        }

        return outcome;
    }
}
