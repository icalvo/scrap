using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scrap.Common;
using SharpX;

namespace Scrap.Domain.Sites;

class SiteFactory : ISiteFactory
{
    private readonly ISiteRepository _sitesRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SiteFactory> _logger;

    public SiteFactory(ISiteRepository sitesRepository, IConfiguration configuration, ILogger<SiteFactory> logger)
    {
        _sitesRepository = sitesRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<Result<Site, Unit>> Build(
        Maybe<NameOrRootUrl> argNameOrRootUrl)
    {
        Debug.Assert(_configuration != null, nameof(_configuration) + " != null");
        var envName = _configuration.SiteName();
        var envRootUrl = _configuration.SiteRootUrl();
        var envNameOrRootUrl = NameOrRootUrl.Create(envName, envRootUrl.TryBuildUri());
        var maybeNameOrUrl = argNameOrRootUrl.OrElse(envNameOrRootUrl);
        
        return maybeNameOrUrl.Map(
                either => either
                    .MatchNameFirst(
                        name => _sitesRepository
                            .GetByNameAsync(name)
                            .ContinueAsync(x => x.ToResult(Unit.Do(() => _logger.LogWarning("Error finding site: No site matches site {Name}", name)))),
                        uri => FindByRootUrl(uri.AbsoluteUri))
                    .ContinueAsync(r => r.AddFailMessage(Unit.Do(
                        () => _logger.LogWarning("No single site was found, nothing will be done")))),
                () =>
                {
                    _logger.LogWarning(
                        "No data was provided for locating a site (name or root URL in arguments or environment)");
                    return Task.FromResult(Result<Site, Unit>.FailWith(Unit.Do(() => {})));
                })
            .PipeAsync(site => _logger.LogInformation("Site: {Site}", site.Name));

        async Task<Result<Site, Unit>> FindByRootUrl(string r)
        {
            var sites = await _sitesRepository.FindByRootUrlAsync(r).ToArrayAsync();

            return sites.Length switch
            {
                0 => Result<Site, Unit>.FailWith(Unit.Do(() => _logger.LogError("No site matches with {RootUrl}", r))),
                > 1 => Result<Site, Unit>.FailWith(Unit.Do(() => _logger.LogError("More than one site matched with {RootUrl}", r))),
                _ => Result<Site, Unit>.Succeed(sites[0])
            };
        }
    }
}
