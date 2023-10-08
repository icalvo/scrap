using Scrap.Common;
using Scrap.Domain.Resources;

namespace Scrap.Domain.Jobs;

public class DownloadJob : IDownloadJob
{
    public DownloadJob(
        AsyncLazy<IResourceRepositoryConfiguration> resourceRepoArgs,
        bool disableResourceWrites,
        int httpRequestRetries,
        TimeSpan httpRequestDelayBetweenRetries,
        bool downloadAlways)
    {
        ResourceRepoArgs = resourceRepoArgs;
        DisableResourceWrites = disableResourceWrites;
        HttpRequestRetries = httpRequestRetries;
        HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries;
        DownloadAlways = downloadAlways;
    }

    public AsyncLazy<IResourceRepositoryConfiguration> ResourceRepoArgs { get; }
    public bool DisableResourceWrites { get; }
    public int HttpRequestRetries { get; }
    public TimeSpan HttpRequestDelayBetweenRetries { get; }
    public bool DownloadAlways { get; }
}