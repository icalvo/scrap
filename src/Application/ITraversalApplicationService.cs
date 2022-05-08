using Scrap.Domain.Jobs;

namespace Scrap.Application;

public interface ITraversalApplicationService
{
    IAsyncEnumerable<string> TraverseAsync(JobDto jobDto);
}
