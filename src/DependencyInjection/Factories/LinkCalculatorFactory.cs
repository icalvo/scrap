using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;

namespace Scrap.DependencyInjection.Factories;

public class LinkCalculatorFactory : IFactory<Job, ILinkCalculator>
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IFactory<Job, IPageMarkerRepository> _pageMarkerRepositoryFactory;

    public LinkCalculatorFactory(
        ILoggerFactory loggerFactory,
        IFactory<Job, IPageMarkerRepository> pageMarkerRepositoryFactory)
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
