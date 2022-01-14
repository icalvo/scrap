using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Scrap
{
    public class AsyncDelayPolicy : AsyncPolicy
    {
        private readonly TimeSpan _delay;

        private AsyncDelayPolicy(TimeSpan delay)
        {
            _delay = delay;
        }

        public static AsyncDelayPolicy Create(TimeSpan delay)
        {
            return new AsyncDelayPolicy(delay);
        }

        protected override async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            await Task.Delay(_delay, cancellationToken);
            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
    }
}
