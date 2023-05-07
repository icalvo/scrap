using Scrap.Domain.Jobs;
using SharpX;

namespace Scrap.Domain.Sites;

public interface ISiteService
{
    Task<Maybe<(Job job, string siteName)>> BuildJobAsync(
        Maybe<NameOrRootUrl> argNameOrRootUrl,
        bool? fullScan,
        bool? downloadAlways,
        bool? disableMarkingVisited,
        bool? disableResourceWrites);

    IAsyncEnumerable<Site> GetAllAsync();

    Task<Job> BuildJobAsync(
        Site site,
        string? argRootUrl = null,
        string? envRootUrl = null,
        bool? fullScan = null,
        bool? downloadAlways = null,
        bool? disableMarkingVisited = null,
        bool? disableResourceWrites = null);
}
