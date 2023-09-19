using Microsoft.Extensions.Logging;
using Scrap.Domain.Jobs;
using Scrap.Domain.Sites;

namespace Scrap.Application.Scrap.All;

public class ScrapAllApplicationService : IScrapAllApplicationService
{
    private readonly IJobBuilder _jobBuilder;
    private readonly ISiteRepository _siteRepository;
    private readonly ILogger<ScrapAllApplicationService> _logger;
    private readonly ISingleScrapService _singleScrapService;

    public ScrapAllApplicationService(
        IJobBuilder jobBuilder,
        ILogger<ScrapAllApplicationService> logger,
        ISingleScrapService singleScrapService,
        ISiteRepository siteRepository)
    {
        _jobBuilder = jobBuilder;
        _logger = logger;
        _singleScrapService = singleScrapService;
        _siteRepository = siteRepository;
    }

    public async Task ScrapAllAsync(IScrapAllCommand command)
    {
        var sites = await _siteRepository.GetScrappableAsync().ToArrayAsync();
        if (!sites.Any())
        {
            _logger.LogWarning("No site found, nothing will be done");
            return;
        }

        _logger.LogInformation(
            "The following sites will be run: {Sites}",
            string.Join(", ", sites.Select(x => x.Name)));
        foreach (var site in sites)
        {
            var job = await _jobBuilder.BuildJobAsync(
                site,
                null,
                null,
                command.FullScan,
                command.DownloadAlways,
                command.DisableMarkingVisited,
                command.DisableResourceWrites);
            try
            {
                await _singleScrapService.ExecuteJobAsync(site.Name, job);
            }
            catch (Exception)
            {
                _logger.LogWarning("Could not scrap site {Site}", site.Name);
            }
        }
    }
}
