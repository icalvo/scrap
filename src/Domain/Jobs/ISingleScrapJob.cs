using Scrap.Common;
using Scrap.Domain.Resources;

namespace Scrap.Domain.Jobs;

public interface ISingleScrapJob : IResourceRepositoryOptions, IResourcesJob, ITraverseJob
{
    public ResourceType ResourceType { get; }
    public bool DownloadAlways { get; }
}
