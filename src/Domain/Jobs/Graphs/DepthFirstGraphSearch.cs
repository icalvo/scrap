namespace Scrap.Domain.Jobs.Graphs;

public class DepthFirstGraphSearch : IGraphSearch
{
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
            var unvisitedAdjacentRefs = adjacentRefs.Where(n => !visitedRefs.Contains(n)).Reverse();
            await foreach (var adjacentRef in unvisitedAdjacentRefs)
            {
                unvisitedRefStack.Push(adjacentRef);
            }
        }
    }
}
