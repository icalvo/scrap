using Polly;

namespace Scrap.Infrastructure.Resilience;

internal sealed class MemoryCacheStrategyOptions : ResilienceStrategyOptions
{
    public MemoryCacheStrategyOptions() => Name = "MemoryCache";
}

internal sealed class FixedDelayStrategyOptions : ResilienceStrategyOptions
{
    public FixedDelayStrategyOptions() => Name = "FixedDelay";
}
