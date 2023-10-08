using Scrap.Common;
using Scrap.Domain.Jobs;

namespace Scrap.Application.Scrap.One;

public class SingleScrapApplicationService : ISingleScrapApplicationService
{
    private readonly ISingleScrapService _singleScrapService;
    private readonly ICommandJobBuilder<ISingleScrapCommand, ISingleScrapJob> _siteFactory;

    public SingleScrapApplicationService(
        ISingleScrapService singleScrapService,
        ICommandJobBuilder<ISingleScrapCommand, ISingleScrapJob> siteFactory)
    {
        _singleScrapService = singleScrapService;
        _siteFactory = siteFactory;
    }

    public Task ScrapAsync(ISingleScrapCommand command) =>
        _siteFactory.Build(command)
            .DoAsync(x => _singleScrapService.ExecuteJobAsync(x.Item2.Name, x.Item1));
}
