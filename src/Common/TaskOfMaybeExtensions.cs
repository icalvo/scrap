using SharpX;

namespace Scrap.Common;

public static class TaskOfMaybeExtensions
{
    public static Task DoAsync<TIn>(this Task<Maybe<TIn>> task, Action<TIn> f) =>
        task.ContinueAsync(x => x.Do(y => Unit.Do(() => f(y))));

    public static Task DoAsync<TIn>(this Task<Maybe<TIn>> task, Func<TIn, Task> f) =>
        task.ContinueWithAsync(x => x.DoAsync(y => Unit.DoAsync(() => f(y))));

    public static Task<Maybe<T>> DoIfNothingAsync<T>(this Task<Maybe<T>> task, Action ifNone) =>
        task.ContinueAsync(maybe => maybe.DoIfNothing(ifNone));

    public static Task<Maybe<TOut>> MapAsync<TIn, TOut>(this Task<Maybe<TIn>> task, Func<TIn, Task<TOut>> f) =>
        task.ContinueWithAsync(
            x => x.Map(y =>
            {
                Task<TOut> task1 = f(y);
                return task1.ContinueAsync(z => z.ToJust());
            }, () => Task.FromResult(Maybe.Nothing<TOut>())));

    public static Task<Maybe<TIn>> PipeAsync<TIn>(this Task<Maybe<TIn>> task, Action<TIn> f) =>
        task.MapAsync<TIn, TIn>(
            x =>
            {
                f(x);
                return x.ToTaskResult();
            });
    
    public static Task<Maybe<TOut>> BindAsync<TIn, TOut>(this Task<Maybe<TIn>> task, Func<TIn, Task<Maybe<TOut>>> f) =>
        task.ContinueWithAsync(x => x.Map(f, () => Task.FromResult(Maybe.Nothing<TOut>())));

    
    public static async IAsyncEnumerable<TOut> MapAsync<TIn, TOut>(
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
}
