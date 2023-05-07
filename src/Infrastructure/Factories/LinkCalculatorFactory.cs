using Microsoft.Extensions.Logging;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Infrastructure.Factories;

public class LinkCalculatorFactory : ILinkCalculatorFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IVisitedPageRepositoryFactory _visitedPageRepositoryFactory;

    public LinkCalculatorFactory(
        ILoggerFactory loggerFactory,
        IVisitedPageRepositoryFactory visitedPageRepositoryFactory)
    {
        _loggerFactory = loggerFactory;
        _visitedPageRepositoryFactory = visitedPageRepositoryFactory;
    }

    public ILinkCalculator Build(Job job) =>
        job.FullScan
            ? new FullScanLinkCalculator(_loggerFactory.CreateLogger<FullScanLinkCalculator>())
            : new LinkCalculator(
                _loggerFactory.CreateLogger<LinkCalculator>(),
                _visitedPageRepositoryFactory.Build(job));
}
