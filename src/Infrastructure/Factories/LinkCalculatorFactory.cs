using Microsoft.Extensions.Logging;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.Infrastructure.Factories;

public class LinkCalculatorFactory : ILinkCalculatorFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IPageMarkerRepositoryFactory _pageMarkerRepositoryFactory;

    public LinkCalculatorFactory(
        ILoggerFactory loggerFactory,
        IPageMarkerRepositoryFactory pageMarkerRepositoryFactory)
    {
        _loggerFactory = loggerFactory;
        _pageMarkerRepositoryFactory = pageMarkerRepositoryFactory;
    }

    public ILinkCalculator Build(Job job) =>
        job.FullScan
            ? new FullScanLinkCalculator(_loggerFactory.CreateLogger<FullScanLinkCalculator>())
            : new LinkCalculator(
                _loggerFactory.CreateLogger<LinkCalculator>(),
                _pageMarkerRepositoryFactory.Build(job));
}
