using SharpX;

namespace Scrap.Common;

public static class TaskOfResultExtensions
{
    public static Task DoAsync<TIn, TM>(this Task<Result<TIn, TM>> task, Action<TIn> f) =>
        task.ContinueAsync(x => x.DoIfSuccess(f));

    public static Task DoAsync<TIn, TM>(this Task<Result<TIn, TM>> task, Func<TIn, Task> f) =>
        task.ContinueAsync(x => x.ToMaybe()).DoAsync(f);

    public static Task<Result<T, TM>> DoIfFailAsync<T, TM>(this Task<Result<T, TM>> task, Action<IEnumerable<TM>> ifNone) =>
        task.ContinueAsync(maybe => maybe.DoIfFail(ifNone));

    public static Task<Result<TOut, TM>> MapAsync<TIn, TOut, TM>(this Task<Result<TIn, TM>> task,
        Func<TIn, Task<TOut>> f) =>
        task.ContinueWithAsync(
            x => x.Map(
                y => f(y).ContinueAsync(Result<TOut, TM>.Succeed),
                m => Task.FromResult(Result<TOut, TM>.FailWith(m))));

    public static Task<Result<TIn, TM>> PipeAsync<TIn, TM>(this Task<Result<TIn, TM>> task, Action<TIn> f) =>
        task.MapAsync<TIn, TIn, TM>(
            x =>
            {
                f(x);
                return x.ToTaskResult();
            });
    
    public static Task<Result<TOut, TM>> BindAsync<TIn, TOut, TM>(this Task<Result<TIn, TM>> task, Func<TIn, Result<TOut, TM>> f) =>
        task.ContinueAsync(x => x.Map(f, Result<TOut, TM>.FailWith));

    
    public static async IAsyncEnumerable<TOut> MapAsync<TIn, TOut, TM>(
        this Task<Result<TIn, TM>> task,
        Func<TIn, IAsyncEnumerable<TOut>> f)
    {
        var x = await task;
        var a = x.Map(f, _ => AsyncEnumerable.Empty<TOut>());
        await foreach (var item in a)
        {
            yield return item;
        }
    }
}
