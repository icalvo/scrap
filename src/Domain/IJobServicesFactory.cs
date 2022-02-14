using Scrap.Downloads;
using Scrap.Jobs;
using Scrap.Pages;
using Scrap.Resources;

namespace Scrap;

public interface IJobServicesFactory
{
    IPageRetriever GetHttpPageRetriever(Job job);
    IPageMarkerRepository GetPageMarkerRepository(Job job);
    Task<IResourceRepository> GetResourceRepositoryAsync(Job job);
    IDownloadStreamProvider GetDownloadStreamProvider(Job job);
    ILinkCalculator GetLinkCalculator(Job job);
}
