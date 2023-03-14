using Scrap.Common;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;

namespace Scrap.Application.Scrap;

public class ScrapApplicationService : IScrapApplicationService
{
    private readonly IAsyncFactory<JobDto, Job> _jobFactory;
    private readonly IScrapDownloadsService _scrapDownloadsService;
    private readonly IScrapTextService _scrapTextService;

    public ScrapApplicationService(
        IAsyncFactory<JobDto, Job> jobFactory,
        IScrapDownloadsService scrapDownloadsService,
        IScrapTextService scrapTextService)
    {
        _jobFactory = jobFactory;
        _scrapDownloadsService = scrapDownloadsService;
        _scrapTextService = scrapTextService;
    }

    public async Task ScrapAsync(JobDto jobDto)
    {
        _ = await _jobFactory.Build(jobDto);
        await (jobDto.ResourceType switch
        {
            ResourceType.DownloadLink => _scrapDownloadsService.DownloadLinksAsync(jobDto),
            ResourceType.Text => _scrapTextService.ScrapTextAsync(jobDto),
            _ => throw new Exception("Invalid resource type")
        });
    }
}
