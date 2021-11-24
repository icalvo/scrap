using Scrap.Downloads;
using Scrap.Jobs;
using Scrap.Pages;
using Scrap.Resources;

namespace Scrap
{
    public interface IJobServicesResolver
    {
        (IDownloadStreamProvider downloadStreamProvider, IResourceRepository resourceRepository, IPageRetriever pageRetriever, IPageMarkerRepository pageMarkerRepository) BuildJobDependencies(Job job);
    }
}
