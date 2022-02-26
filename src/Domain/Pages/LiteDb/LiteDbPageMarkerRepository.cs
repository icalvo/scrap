using LiteDB;
using Microsoft.Extensions.Logging;
using Scrap.Domain.Jobs;

namespace Scrap.Domain.Pages.LiteDb;

public class LiteDbPageMarkerRepository : IPageMarkerRepository
{
    private readonly ILogger<LiteDbPageMarkerRepository> _logger;
    private readonly bool _disableWrites;
    private readonly ILiteCollection<PageMarker> _collection;

    public LiteDbPageMarkerRepository(
        ILiteDatabase db,
        ILogger<LiteDbPageMarkerRepository> logger,
        Job job)
    {
        _logger = logger;
        _disableWrites = job.DisableMarkingVisited;
        _collection = db.GetCollection<PageMarker>();
    }
        
    public Task<bool> ExistsAsync(Uri uri)
    {
        var findById = _collection.FindById(uri.AbsoluteUri);
        return Task.FromResult(findById != null);
    }

    public Task UpsertAsync(Uri link)
    {
        if (!_disableWrites)
        {
            _collection.Upsert(link.AbsoluteUri, new PageMarker(link.AbsoluteUri));
            _logger.LogTrace("Inserted {Page}", link.AbsoluteUri);
        }
        else
        {
            _logger.LogTrace("FAKE. Inserted {Page}", link.AbsoluteUri);
        }

        return Task.CompletedTask;
    }
}
