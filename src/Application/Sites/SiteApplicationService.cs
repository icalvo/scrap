using Microsoft.Extensions.Logging;
using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Application.Sites;

public class SiteApplicationService : ISiteApplicationService
{
    private readonly ISiteRepository _siteRepository;
    private readonly ILogger<SiteApplicationService> _logger;

    public SiteApplicationService(ISiteRepository siteRepository, ILogger<SiteApplicationService> logger)
    {
        _siteRepository = siteRepository;
        _logger = logger;
    }

    public async Task<Maybe<SiteDto>> FindByNameAsync(string siteName)
    {
        _logger.LogDebug("Getting site called {Site}", siteName);
        return (await _siteRepository.GetByNameAsync(siteName)).Map(x => x.ToDto());
    }

    public IAsyncEnumerable<SiteDto> GetAllAsync()
    {
        _logger.LogDebug("Getting all sites");
        return _siteRepository.ListAsync().Select(x => x.ToDto());
    }

    public IAsyncEnumerable<SiteDto> FindByRootUrlAsync(string rootUrl) =>
        _siteRepository.FindByRootUrlAsync(rootUrl).Select(x => x.ToDto());
}
