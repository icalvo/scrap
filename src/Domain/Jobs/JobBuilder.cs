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
        return GetSiteAsync(
            argNameOrRootUrl,
            NameOrRootUrl.Create(envName, envRootUrl == null ? null : new Uri(envRootUrl))).MapAsync(
            async site =>
            {
                _logger.LogInformation("Site: {Site}", site.Name);

                var jobDto = await BuildJobAsync(
                    site,
                    argRootUrl,
                    envRootUrl,
                    fullScan,
                    downloadAlways,
                    disableMarkingVisited,
                    disableResourceWrites);
                return (jobDto, site.Name);
            });
    }


    public async Task<Job> BuildJobAsync(
        Site site,
        string? argRootUrl,
        string? envRootUrl = null,
        bool? fullScan = null,
        bool? downloadAlways = null,
        bool? disableMarkingVisited = null,
        bool? disableResourceWrites = null)
    {
        await _repositoryConfigurationValidator.ValidateAsync(site.ResourceRepoArgs);
        return new Job(
            site,
            (argRootUrl == null ? null : new Uri(argRootUrl)) ?? (envRootUrl == null ? null : new Uri(envRootUrl)),
            fullScan,
            null,
            downloadAlways,
            disableMarkingVisited,
            disableResourceWrites);
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
