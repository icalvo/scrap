using Microsoft.Extensions.Logging;

namespace Scrap.Common.Graphs;

public class DepthFirstGraphSearch : IGraphSearch
{
    private readonly ILogger<DepthFirstGraphSearch> _logger;

    public DepthFirstGraphSearch(ILogger<DepthFirstGraphSearch> logger)
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<T> SearchAsync<TRef, T>(
        TRef startRef,
        Func<TRef, Task<T>> visit,
        Func<T, IAsyncEnumerable<TRef>> adj)
    {
        HashSet<TRef> visitedRefs = new(EqualityComparer<TRef>.Default);
        Stack<TRef> unvisitedRefStack = new();
        unvisitedRefStack.Push(startRef);
        while (unvisitedRefStack.Any())
        {
            var currentRef = unvisitedRefStack.Pop();
            if (visitedRefs.Contains(currentRef))
            {
                continue;
            }

            var currentNode = await visit(currentRef);
            visitedRefs.Add(currentRef);
            yield return currentNode;

            var adjacentRefs = adj(currentNode);
            var unvisitedAdjacentRefs =
                await adjacentRefs.Where(n => !visitedRefs.Contains(n)).Reverse().ToArrayAsync();
            foreach (var adjacentRef in unvisitedAdjacentRefs)
            {
                unvisitedRefStack.Push(adjacentRef);
            }

            _logger.LogInformation(
                "{UnvisitedCount} unvisited refs found, {UnvisitedTotal} total",
                unvisitedAdjacentRefs.Length,
                unvisitedRefStack.Count);
        }
    }
}
