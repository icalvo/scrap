using Scrap.Domain.Jobs;

namespace Scrap.Application;

public interface IResourcesApplicationService
{
    IAsyncEnumerable<string> GetResourcesAsync(NewJobDto jobDto, Uri pageUrl, int pageIndex);
}