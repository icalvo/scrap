using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain.Resources;
using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Domain.Jobs;

public class JobBuilder : IJobBuilder
{
    private readonly ISiteRepository _sitesRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JobBuilder> _logger;
    private readonly IResourceRepositoryConfigurationValidator _repositoryConfigurationValidator;

    public JobBuilder(
        ISiteRepository sitesRepository,
        IConfiguration configuration,
        IResourceRepositoryConfigurationValidator repositoryConfigurationValidator,
        ILogger<JobBuilder> logger)
    {
        _sitesRepository = sitesRepository;
        _configuration = configuration;
        _logger = logger;
        _repositoryConfigurationValidator = repositoryConfigurationValidator;
    }

    public Task<Maybe<(Job, string)>> BuildJobAsync(
        Maybe<NameOrRootUrl> argNameOrRootUrl,
        bool? fullScan,
        bool? downloadAlways,
        bool? disableMarkingVisited,
        bool? disableResourceWrites)
    {
        Debug.Assert(_configuration != null, nameof(_configuration) + " != null");
        var envName = _configuration.SiteName();
        var envRootUrl = _configuration.SiteRootUrl();

        var argRootUrl = argNameOrRootUrl.Map(nr => nr.MatchRootUrl(x => x.AbsoluteUri), Maybe.Nothing<string>)
            .FromJust();

        return GetSiteAsync(argNameOrRootUrl, NameOrRootUrl.Create(envName, envRootUrl.TryBuildUri())).MapAsync(
            site =>
            {
                _logger.LogInformation("Site: {Site}", site.Name);
                return site.ToTaskResult();
            }).BindAsync(
            site => BuildJob(
                    site,
                    argRootUrl,
                    fullScan,
                    downloadAlways,
                    disableMarkingVisited,
                    disableResourceWrites).Map(j => (jobDto: j, site.Name)).ToTaskResult());
    }

    public Task<Maybe<(IDownloadJob, string)>> BuildDownloadJob(
        Maybe<NameOrRootUrl> argNameOrRootUrl,
        bool downloadAlways,
        bool disableResourceWrites)
    {
        Debug.Assert(_configuration != null, nameof(_configuration) + " != null");
        var envName = _configuration.SiteName();
        var envRootUrl = _configuration.SiteRootUrl();

        var argRootUrl = argNameOrRootUrl.Map(nr => nr.MatchRootUrl(x => x.AbsoluteUri), Maybe.Nothing<string>)
            .FromJust();

        return GetSiteAsync(argNameOrRootUrl, NameOrRootUrl.Create(envName, envRootUrl.TryBuildUri())).MapAsync(
            site =>
            {
                _logger.LogInformation("Site: {Site}", site.Name);
                return site.ToTaskResult();
            }).BindAsync(
            site => BuildDownloadJob(site, argRootUrl, downloadAlways, disableResourceWrites)
                .Map(j => (job: j, site.Name)).ToTaskResult());
    }

    public Maybe<IDownloadJob> BuildDownloadJob(
        Site site,
        string? argRootUrl,
        bool downloadAlways,
        bool disableResourceWrites)
    {
        var rootUrl = argRootUrl.TryBuildUri() ?? argRootUrl.TryBuildUri() ?? site.RootUrl;
        if (rootUrl == null)
        {
            return Maybe.Nothing<IDownloadJob>();
        }

        if (site.HttpRequestRetries == null)
        {
            return Maybe.Nothing<IDownloadJob>();
        }

        if (site.HttpRequestDelayBetweenRetries == null)
        {
            return Maybe.Nothing<IDownloadJob>();
        }

        var job = new DownloadJob(
            AsyncLazy.Create(
                async () =>
                {
                    await _repositoryConfigurationValidator.ValidateAsync(site.ResourceRepoArgs);
                    return site.ResourceRepoArgs ?? throw new ArgumentNullException();
                }),
            disableResourceWrites,
            site.HttpRequestRetries.Value,
            site.HttpRequestDelayBetweenRetries.Value,
            downloadAlways);
        return Maybe.Just<IDownloadJob>(job);
    }

    public Maybe<Job> BuildJob(
        Site site,
        string? argRootUrl,
        bool? fullScan = null,
        bool? downloadAlways = null,
        bool? disableMarkingVisited = null,
        bool? disableResourceWrites = null)
    {
        var rootUrl = argRootUrl.TryBuildUri() ?? argRootUrl.TryBuildUri() ?? site.RootUrl;
        if (rootUrl == null)
        {
            return Maybe.Nothing<Job>();
        }

        var job = new Job(
            rootUrl,
            site.ResourceType,
            AsyncLazy.Create(
                async () =>
                {
                    await _repositoryConfigurationValidator.ValidateAsync(site.ResourceRepoArgs);
                    return site.ResourceRepoArgs ?? throw new ArgumentNullException();
                }),
            site.AdjacencyXPath,
            site.ResourceXPath,
            site.HttpRequestRetries,
            site.HttpRequestDelayBetweenRetries,
            fullScan,
            downloadAlways,
            disableMarkingVisited,
            disableResourceWrites);
        return Maybe.Just(job);
    }

    private Task<Maybe<Site>> GetSiteAsync(Maybe<NameOrRootUrl> argNameOrRootUrl, Maybe<NameOrRootUrl> envNameOrRootUrl)
    {
        var maybeNameOrUrl = argNameOrRootUrl.OrElse(envNameOrRootUrl);

        return maybeNameOrUrl.Map(
            either => either
                .MatchNameFirst(
                    n => _sitesRepository.GetByNameAsync(n)
                        .DoIfNothingAsync(() => _logger.LogWarning("Couldn't find site {Name}", n)),
                    uri => FindByRootUrl(uri.AbsoluteUri)).DoIfNothingAsync(
                    () => _logger.LogWarning("No single site was found, nothing will be done")),
            () =>
            {
                _logger.LogWarning(
                    "No data was provided for locating a site (name or root URL in arguments or environment)");
                return Task.FromResult(Maybe.Nothing<Site>());
            });

        async Task<Maybe<Site>> FindByRootUrl(string r)
        {
            var sites = await _sitesRepository.FindByRootUrlAsync(r).ToArrayAsync();
            switch (sites.Length)
            {
                case 0:
                    _logger.LogError("No site matches with {RootUrl}", r);
                    return Maybe.Nothing<Site>();
                case > 1:
                    _logger.LogError("More than one site matched with {RootUrl}", r);
                    return Maybe.Nothing<Site>();
                default:
                    return Maybe.Just(sites[0]);
            }
        }
    }
}
