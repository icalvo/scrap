using Scrap.Common;
using Scrap.Domain.Jobs;

namespace Scrap.Application.Scrap.One;

public class ScrapOneApplicationService : IScrapOneApplicationService
{
    private readonly IJobBuilder _jobBuilder;
    private readonly ISingleScrapService _singleScrapService;

    public ScrapOneApplicationService(IJobBuilder jobBuilder, ISingleScrapService singleScrapService)
    {
        _jobBuilder = jobBuilder;
        _singleScrapService = singleScrapService;
    }

    public Task ScrapAsync(IScrapOneCommand oneCommand) =>
        _jobBuilder.BuildJobAsync(
            oneCommand.NameOrRootUrl,
            oneCommand.FullScan,
            oneCommand.DownloadAlways,
            oneCommand.DisableMarkingVisited,
            oneCommand.DisableResourceWrites).DoAsync(x => _singleScrapService.ExecuteJobAsync(x.siteName, x.job));
}
