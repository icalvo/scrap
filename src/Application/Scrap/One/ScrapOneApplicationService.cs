using Scrap.Common;
using Scrap.Domain.Jobs;

namespace Scrap.Application.Scrap.One;

public class ScrapOneApplicationService : IScrapOneApplicationService
{
    private readonly IJobService _siteService;
    private readonly ISingleScrapService _singleScrapService;

    public ScrapOneApplicationService(IJobService siteService, ISingleScrapService singleScrapService)
    {
        _siteService = siteService;
        _singleScrapService = singleScrapService;
    }

    public Task ScrapAsync(IScrapOneCommand oneCommand) =>
        _siteService
            .BuildJobAsync(
                oneCommand.NameOrRootUrl,
                oneCommand.FullScan,
                oneCommand.DownloadAlways,
                oneCommand.DisableMarkingVisited,
                oneCommand.DisableResourceWrites).DoAsync(x => _singleScrapService.ExecuteJobAsync(x.siteName, x.job));
}
