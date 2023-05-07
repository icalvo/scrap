using SharpX;

namespace Scrap.Common;

public static class MaybeExtensions
{
    public static TState Fold<T, TState>(this Maybe<T> first, Func<TState, T, TState> folder, TState state) =>
        first.Map(x => folder(state, x), () => state);

    public static Maybe<T> OrElse<T>(this Maybe<T> first, Maybe<T> ifNone) => first.Map(Maybe.Just, () => ifNone);

    public static Maybe<T> DoIfNothing<T>(this Maybe<T> maybe, Action ifNone)
    {
        if (maybe.IsNothing())
        {
            ifNone();
        }

        return maybe;
    }

    public static Maybe<T> Do<T>(this Maybe<T> maybe, Action<T> ifJust, Action ifNone)
    {
        maybe.Do(x => Unit.Do(() => ifJust(x)));
        if (maybe.IsNothing())
        {
            ifNone();
        }

        return maybe;
    }

    public static async Task<TOut> ContinueAsync<TIn, TOut>(this Task<TIn> task, Func<TIn, TOut> f)
    {
        var result = await task;
        return f(result);
    }

    public static async Task<TOut> ContinueWithAsync<TIn, TOut>(this Task<TIn> task, Func<TIn, Task<TOut>> f)
    {
        var result = await task;
        return await f(result);
    }

    public static async Task ContinueAsync<TIn>(this Task<TIn> task, Action<TIn> f)
    {
        var result = await task;
        f(result);
    }

    public static async Task ContinueWithAsync<TIn>(this Task<TIn> task, Func<TIn, Task> f)
    {
        var result = await task;
        await f(result);
    }
}
