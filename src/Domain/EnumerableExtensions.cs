using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scrap
{
    public static class EnumerableExtensions
    {
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
    }
}
