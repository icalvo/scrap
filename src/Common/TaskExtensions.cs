namespace Scrap.Common;

public static class TaskExtensions
{
    public static Task<T> ToTaskResult<T>(this T result) => Task.FromResult(result);
    
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
