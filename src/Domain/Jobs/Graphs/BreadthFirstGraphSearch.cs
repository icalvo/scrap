namespace Scrap.Domain.Jobs.Graphs;

public class BreadthFirstGraphSearch : IGraphSearch
{
    public async IAsyncEnumerable<T> SearchAsync<TRef, T>(TRef startRef, Func<TRef, Task<T>> visit, Func<T, IAsyncEnumerable<TRef>> adjacentRefs)
    {
        var visitedRefs = new HashSet<TRef>();
        Queue<T> queue = new();
     
        var startNode = await visit(startRef);
        visitedRefs.Add(startRef);
        yield return startNode;
        queue.Enqueue(startNode);        
 
        while(queue.Any())
        {
         
            var currentNode = queue.Dequeue();
            var adjRefs = adjacentRefs(currentNode);
 
            await foreach (var adjRef in adjRefs.Where(adj => !visitedRefs.Contains(adj)))
            {
                var adjNode = await visit(adjRef);
                yield return adjNode;
                visitedRefs.Add(adjRef);
                queue.Enqueue(adjNode);
            }
        }
    }
}