using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace System.Runtime.CompilerServices
{
}

namespace Scrap
{
    public class AsyncDelayPolicy : AsyncPolicy
    {
        private readonly TimeSpan _delay;

        public AsyncDelayPolicy(TimeSpan delay)
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