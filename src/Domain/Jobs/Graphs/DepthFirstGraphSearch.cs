namespace Scrap.Domain.Jobs.Graphs;

public class DepthFirstGraphSearch : IGraphSearch
{
   
    public async IAsyncEnumerable<T> SearchAsync<TRef, T>(TRef startRef, Func<TRef, Task<T>> visit, Func<T, IAsyncEnumerable<TRef>> adj)
    {
        HashSet<TRef> visitedRefs = new(EqualityComparer<TRef>.Default);
        Stack<TRef> stack = new();
        stack.Push(startRef);
        while (stack.Any())
        {
            var currentRef = stack.Pop();
            if (visitedRefs.Contains(currentRef))
            {
                continue;
            }

            var currentNode = await visit(currentRef);
            visitedRefs.Add(currentRef);
            yield return currentNode;

            var unvisitedAdjacent = adj(currentNode).Where(n => !visitedRefs.Contains(n)).Reverse();
            await foreach (var adjacentNode in unvisitedAdjacent)
            {
                stack.Push(adjacentNode);
            }
        }
    }        
}