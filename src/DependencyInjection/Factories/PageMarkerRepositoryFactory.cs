using LiteDB;
using Microsoft.Extensions.Logging;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Pages.LiteDb;

namespace Scrap.DependencyInjection.Factories;

public class PageMarkerRepositoryFactory : ISingleOptionalParameterFactory<Job, IPageMarkerRepository>
{
    private readonly ILiteCollection<PageMarker> _liteCollection;
    private readonly ILoggerFactory _loggerFactory;

    public PageMarkerRepositoryFactory(ILiteDatabase liteDatabase, ILoggerFactory loggerFactory)
    {
        _liteCollection = liteDatabase.GetCollection<PageMarker>();
        _loggerFactory = loggerFactory;
    }

    public IPageMarkerRepository Build(Job job) => Build(job.DisableMarkingVisited);

    public IPageMarkerRepository Build() => Build(false);

    private IPageMarkerRepository Build(bool disableMarkingVisited) =>
        new LiteDbPageMarkerRepository(
            _liteCollection,
            _loggerFactory.CreateLogger<LiteDbPageMarkerRepository>(),
            disableMarkingVisited);
}
