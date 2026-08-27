using Polly;

namespace Scrap.Infrastructure.Resilience;

internal sealed class FixedDelayResilienceStrategy : ResilienceStrategy
{
    private readonly TimeSpan _delay;

    public FixedDelayResilienceStrategy(TimeSpan delay) => _delay = delay;

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        await Task.Delay(_delay, context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);
        return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
    }
}
