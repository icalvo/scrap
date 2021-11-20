using Scrap.Downloads;
using Scrap.Jobs;
using Scrap.Pages;
using Scrap.Resources;

namespace Scrap
{
    public interface IJobServicesResolver
    {
        (IDownloadStreamProvider downloadStreamProvider, IResourceRepository resourceRepository, LinkedPagesCalculator adjacencyCalculator, IPageRetriever pageRetriever) Build(Job job);
    }
}
