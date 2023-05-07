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

    public static async Task ContinueAsync<TIn, TOut>(this Task<TIn> task, Action<TIn> f)
    {
        var result = await task;
        f(result);
    }

    public static async Task ContinueWithAsync<TIn, TOut>(this Task<TIn> task, Func<TIn, Task> f)
    {
        var result = await task;
        await f(result);
    }

    public static Task DoWithAsync<TIn>(this Task<Maybe<TIn>> task, Func<TIn, Task> f) =>
        task.ContinueWithAsync(x => x.DoAsync(y => Unit.DoAsync(() => f(y))));

    public static Task<Maybe<TOut>> MapWithAsync<TIn, TOut>(this Task<Maybe<TIn>> task, Func<TIn, Task<TOut>> f) =>
        task.ContinueWithAsync(
            x => x.Map(y => f(y).ContinueAsync(z => z.ToJust()), () => Task.FromResult(Maybe.Nothing<TOut>())));

    public static async IAsyncEnumerable<TOut> MapWithAsync<TIn, TOut>(
        this Task<Maybe<TIn>> task,
        Func<TIn, IAsyncEnumerable<TOut>> f)
    {
        var x = await task;
        var a = x.Map(f, AsyncEnumerable.Empty<TOut>);
        await foreach (var item in a)
        {
            yield return item;
        }
    }

    public static Task<Maybe<T>> DoIfNothingAsync<T>(this Task<Maybe<T>> task, Action ifNone) =>
        task.ContinueAsync(maybe => maybe.DoIfNothing(ifNone));
}
