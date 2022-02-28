using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;

namespace Scrap.Application.Scrap;

public class ScrapApplicationService : IScrapApplicationService
{
    private readonly IJobFactory _jobFactory;
    private readonly IScrapDownloadsService _scrapDownloadsService;
    private readonly IScrapTextService _scrapTextService;

    public ScrapApplicationService(
        IJobFactory jobFactory,
        IScrapDownloadsService scrapDownloadsService,
        IScrapTextService scrapTextService)
    {
        _jobFactory = jobFactory;
        _scrapDownloadsService = scrapDownloadsService;
        _scrapTextService = scrapTextService;
    }

    public async Task ScrapAsync(JobDto jobDto)
    {
        _ = await _jobFactory.CreateAsync(jobDto);
        await (jobDto.ResourceType switch
        {
            ResourceType.DownloadLink => _scrapDownloadsService.DownloadLinksAsync(jobDto),
            ResourceType.Text => _scrapTextService.ScrapTextAsync(jobDto),
            _ => throw new Exception($"Invalid resource type")
        });
    }
}
