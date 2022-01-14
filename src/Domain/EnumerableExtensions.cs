using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scrap;

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

    public static async IAsyncEnumerable<T> DoAwait<T, TOut>(this IAsyncEnumerable<T> source, Func<T, IAsyncEnumerable<TOut>> action)
    {
        await foreach (var item in source)
        {
            await action(item).ExecuteAsync();
            yield return item;
        }
    }

    public static async IAsyncEnumerable<T> DoAwait<T, TOut>(this IAsyncEnumerable<T> source, Func<T, int, IAsyncEnumerable<TOut>> action)
    {
        var i = 0;
        await foreach (var item in source)
        {
            await action(item, i).ExecuteAsync();
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

    public static Task ExecuteAsync<T>(this IAsyncEnumerable<T> source)
    {
        return source.ForEachAsync((_ => { }));
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> itemAction, Action? noElementsAction)
    {
        var i = 0;
        foreach (var item in source)
        {
            itemAction(item);
            i++;
        }

        if (i == 0)
        {
            noElementsAction?.Invoke();
        }
    }

    public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> source, Action<T> itemAction, Action noElementsAction)
    {
        var i = 0;
        await foreach (var item in source)
        {
            itemAction(item);
            i++;
        }

        if (i == 0)
        {
            noElementsAction();
        }
    }

    public static async Task ForEachAwaitAsync<T>(this IEnumerable<T> source, Func<T, Task> itemAction, Func<Task> noElementsAction)
    {
        var i = 0;
        foreach (var item in source)
        {
            await itemAction(item);
            i++;
        }

        if (i == 0)
        {
            await noElementsAction();
        }
    }

    public static async Task ForEachAwaitAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task> itemAction, Func<Task> noElementsAction)
    {
        var i = 0;
        await foreach (var item in source)
        {
            await itemAction(item);
            i++;
        }

        if (i == 0)
        {
            await noElementsAction();
        }
    }

    public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> source) where T: struct
    {
        return from item in source where item != null select item.Value;
    }
 
    public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> source) where T: class
    {
        return from item in source where item != null select item;
    }       
}