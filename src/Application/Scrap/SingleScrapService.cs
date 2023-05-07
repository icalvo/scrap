using Microsoft.Extensions.Logging;
using Scrap.Domain;
using Scrap.Domain.Jobs;

namespace Scrap.Application.Scrap;

public class SingleScrapService : ISingleScrapService
{
    private readonly IScrapDownloadsService _scrapDownloadsService;
    private readonly IScrapTextService _scrapTextService;
    private readonly ILogger<SingleScrapService> _logger;

    public SingleScrapService(
        IScrapDownloadsService scrapDownloadsService,
        IScrapTextService scrapTextService,
        ILogger<SingleScrapService> logger)
    {
        _scrapDownloadsService = scrapDownloadsService;
        _scrapTextService = scrapTextService;
        _logger = logger;
    }

    public async Task ExecuteJobAsync(string siteName, Job job)
    {
        _logger.LogInformation("Starting {Site}...", siteName);
        await (job.ResourceType switch
        {
            ResourceType.DownloadLink => _scrapDownloadsService.DownloadLinksAsync(job),
            ResourceType.Text => _scrapTextService.ScrapTextAsync(job),
            _ => throw new Exception("Invalid resource type")
        });
        _logger.LogInformation("Finished!");
    }
}
