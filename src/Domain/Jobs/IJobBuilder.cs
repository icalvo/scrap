using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Domain.Jobs;

public interface IJobBuilder
{
    Maybe<IDownloadJob> BuildDownloadJob(
        Site site,
        Maybe<Uri> argRootUrl,
        bool downloadAlways,
        bool disableResourceWrites);

    Maybe<ISingleScrapJob> BuildSingleScrapJob(
        Site site,
        Maybe<Uri> argRootUrl,
        bool fullScan,
        bool downloadAlways,
        bool disableMarkingVisited,
        bool disableResourceWrites);

    Maybe<IResourcesJob> BuildResourcesJob(Site site, Maybe<Uri> argRootUrl);

    Maybe<ITraverseJob> BuildTraverseJob(Site site, Maybe<Uri> argRootUrl, bool disableMarkingVisited, bool fullScan);
}
