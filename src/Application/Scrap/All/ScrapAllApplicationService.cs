﻿using Microsoft.Extensions.Logging;
using Scrap.Domain.Sites;

namespace Scrap.Application.Scrap.All;

public class ScrapAllApplicationService : IScrapAllApplicationService
{
    private readonly ISiteService _sitesService;
    private readonly ILogger<ScrapAllApplicationService> _logger;
    private readonly ISingleScrapService _singleScrapService;

    public ScrapAllApplicationService(
        ISiteService sitesService,
        ILogger<ScrapAllApplicationService> logger,
        ISingleScrapService singleScrapService)
    {
        _sitesService = sitesService;
        _logger = logger;
        _singleScrapService = singleScrapService;
    }

    public async Task ScrapAllAsync(IScrapAllCommand command)
    {
        var sites = await _sitesService.GetAllAsync().Where(x => x.RootUrl != null && x.HasResourceCapabilities())
            .ToArrayAsync();
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
            var job = await _sitesService.BuildJobAsync(
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
