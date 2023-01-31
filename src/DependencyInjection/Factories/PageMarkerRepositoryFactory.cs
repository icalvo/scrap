using LiteDB;
using Microsoft.Extensions.Logging;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Pages.LiteDb;

namespace Scrap.DependencyInjection.Factories;

public class PageMarkerRepositoryFactory : ISingleOptionalParameterFactory<Job, IPageMarkerRepository>
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILiteCollection<PageMarker> _liteCollection;

    public PageMarkerRepositoryFactory(ILiteDatabase liteDatabase, ILoggerFactory loggerFactory)
    {
        _liteCollection = liteDatabase.GetCollection<PageMarker>();
        _loggerFactory = loggerFactory;
    }

    public IPageMarkerRepository Build(Job job) => Build(job.DisableMarkingVisited);
    public IPageMarkerRepository Build() => Build(false);

    private IPageMarkerRepository Build(bool disableMarkingVisited)
    {
        return new LiteDbPageMarkerRepository(
            _liteCollection,
            _loggerFactory.CreateLogger<LiteDbPageMarkerRepository>(),
            disableMarkingVisited);
    }
}
