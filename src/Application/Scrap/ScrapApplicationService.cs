using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;

namespace Scrap.Application.Scrap;

public class ScrapApplicationService : IScrapApplicationService
{
    private readonly IScrapDownloadsService _scrapDownloadsService;
    private readonly IScrapTextService _scrapTextService;

    public ScrapApplicationService(
        IScrapDownloadsService scrapDownloadsService,
        IScrapTextService scrapTextService)
    {
        _scrapDownloadsService = scrapDownloadsService;
        _scrapTextService = scrapTextService;
    }

    public async Task ScrapAsync(NewJobDto jobDto)
    {
        await (jobDto.ResourceType switch
        {
            ResourceType.DownloadLink => _scrapDownloadsService.DownloadLinksAsync(jobDto),
            ResourceType.Text => _scrapTextService.ScrapTextAsync(jobDto),
            _ => throw new Exception($"Invalid resource type")
        });
    }
}
