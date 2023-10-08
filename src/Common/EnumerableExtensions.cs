namespace Scrap.Common;

public static class EnumerableExtensions
{
    public static async IAsyncEnumerable<T> Do<T>(this IAsyncEnumerable<T> source, Action<T> action)
    {
        await foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    public static async IAsyncEnumerable<T> DoIf<T>(this IAsyncEnumerable<T> source, Func<T, bool> condition, Action<T> action)
    {
        await foreach (var item in source)
        {
            if (condition(item))
                action(item);
            yield return item;
        }
    }

    public static async IAsyncEnumerable<T> DoAwait<T>(this IAsyncEnumerable<T> source, Func<T, int, Task> action)
    {
        var i = 0;
        await foreach (var item in source)
        {
            await action(item, i);
            yield return item;
            i++;
        }
    }

    public static async IAsyncEnumerable<T> DoAwait<T>(this IAsyncEnumerable<T> source, Func<T, Task> action)
    {
        await foreach (var item in source)
        {
            await action(item);
            yield return item;
        }
    }

    public static Task ExecuteAsync<T>(this IAsyncEnumerable<T> source) => source.ForEachAsync(_ => { });

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> itemAction, Action noElementsAction)
    {
        var i = 0;
        foreach (var item in source)
        {
            itemAction(item);
            i++;
        }

        if (i == 0)
        {
            noElementsAction.Invoke();
        }
    }

    public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> source) where T : struct =>
        from item in source where item != null select item.Value;

    public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> source) where T : class =>
        from item in source where item != null select item;
}
