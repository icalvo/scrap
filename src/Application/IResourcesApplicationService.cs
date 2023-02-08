using Scrap.Domain.Jobs;

namespace Scrap.Application;

public interface IResourcesApplicationService
{
    IAsyncEnumerable<string> GetResourcesAsync(JobDto jobDto, Uri pageUrl, int pageIndex);
}
