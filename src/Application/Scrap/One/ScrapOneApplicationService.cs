using Scrap.Common;
using Scrap.Domain.Sites;

namespace Scrap.Application.Scrap.One;

public class ScrapOneApplicationService : IScrapOneApplicationService
{
    private readonly ISiteService _sitesService;
    private readonly ISingleScrapService _singleScrapService;

    public ScrapOneApplicationService(ISiteService sitesService, ISingleScrapService singleScrapService)
    {
        _sitesService = sitesService;
        _singleScrapService = singleScrapService;
    }

    public Task ScrapAsync(IScrapOneCommand oneCommand) =>
        _sitesService
            .BuildJobAsync(
                oneCommand.NameOrRootUrl,
                oneCommand.FullScan,
                oneCommand.DownloadAlways,
                oneCommand.DisableMarkingVisited,
                oneCommand.DisableResourceWrites).DoAsync(x => _singleScrapService.ExecuteJobAsync(x.siteName, x.job));
}
