using Scrap.Common;
using Scrap.Domain.Resources;
using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Domain.Jobs;

public class JobBuilder : IJobBuilder
{
    public const int DefaultHttpRequestRetries = 5;
    public static readonly TimeSpan DefaultHttpRequestDelayBetweenRetries = TimeSpan.FromSeconds(1);

    private readonly IResourceRepositoryConfigurationValidator _repositoryConfigurationValidator;

    public JobBuilder(IResourceRepositoryConfigurationValidator repositoryConfigurationValidator)
    {
        _repositoryConfigurationValidator = repositoryConfigurationValidator;
    }

    public Maybe<IDownloadJob> BuildDownloadJob(
        Site site,
        Maybe<Uri> argRootUrl,
        bool downloadAlways,
        bool disableResourceWrites)
    {
        return from rootUrl in argRootUrl.OrElse(site.RootUrl)
            from resourceRepoArgs in site.ResourceRepoArgs
            select new DownloadJob(
                AsyncLazy.Create(
                    async () =>
                    {
                        await _repositoryConfigurationValidator.ValidateAsync(resourceRepoArgs);
                        return resourceRepoArgs;
                    }),
                disableResourceWrites,
                site.HttpRequestRetries.FromJust(DefaultHttpRequestRetries),
                site.HttpRequestDelayBetweenRetries.FromJust(DefaultHttpRequestDelayBetweenRetries),
                downloadAlways) as IDownloadJob;
    }

    public Maybe<ISingleScrapJob> BuildSingleScrapJob(
        Site site,
        Maybe<Uri> argRootUrl,
        bool fullScan,
        bool downloadAlways,
        bool disableMarkingVisited,
        bool disableResourceWrites)
    {
        return from rootUrl in argRootUrl.OrElse(site.RootUrl)
            from resourceXPath in site.ResourceXPath
            from resourceRepoArgs in site.ResourceRepoArgs
            select new SingleScrapJob(
            rootUrl,
            site.ResourceType,
            AsyncLazy.Create(
                async () =>
                {
                    await _repositoryConfigurationValidator.ValidateAsync(resourceRepoArgs);
                    return resourceRepoArgs;
                }),
            site.AdjacencyXPath,
            resourceXPath,
            site.HttpRequestRetries.FromJust(DefaultHttpRequestRetries),
            site.HttpRequestDelayBetweenRetries.FromJust(DefaultHttpRequestDelayBetweenRetries),
            fullScan,
            downloadAlways,
            disableMarkingVisited,
            disableResourceWrites) as ISingleScrapJob;
    }


    public Maybe<ITraverseJob>
        BuildTraverseJob(Site site, Maybe<Uri> argRootUrl, bool disableMarkingVisited, bool fullScan)
    {
        return from rootUrl in argRootUrl.OrElse(site.RootUrl)
            select new TraverseJob(
                rootUrl,
                site.AdjacencyXPath,
                site.HttpRequestRetries.FromJust(DefaultHttpRequestRetries),
                site.HttpRequestDelayBetweenRetries.FromJust(DefaultHttpRequestDelayBetweenRetries),
                disableMarkingVisited,
                fullScan) as ITraverseJob;
    }

    public Maybe<IResourcesJob> BuildResourcesJob(Site site, Maybe<Uri> argRootUrl)
    {
        return from rootUrl in argRootUrl.OrElse(site.RootUrl)
            from resourceXPath in site.ResourceXPath
            select new ResourcesJob(
                site.HttpRequestRetries.FromJust(DefaultHttpRequestRetries),
                site.HttpRequestDelayBetweenRetries.FromJust(DefaultHttpRequestDelayBetweenRetries),
                resourceXPath) as IResourcesJob;
    }
}
