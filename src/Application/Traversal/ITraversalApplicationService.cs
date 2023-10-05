namespace Scrap.Application.Traversal;

public interface ITraversalApplicationService
{
    IAsyncEnumerable<string> TraverseAsync(ITraverseCommand command);
}
