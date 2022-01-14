using LiteDB;
using Microsoft.Extensions.Logging;

namespace Scrap.Pages.LiteDb;

public class LiteDbPageMarkerRepository : IPageMarkerRepository
{
    private readonly ILogger<LiteDbPageMarkerRepository> _logger;
    private readonly bool _disableExists;
    private readonly bool _disableWrites;
    private readonly ILiteCollection<PageMarker> _collection;

    public LiteDbPageMarkerRepository(ILiteDatabase db, ILogger<LiteDbPageMarkerRepository> logger,
        bool disableExists, bool disableWrites)
    {
        _logger = logger;
        _disableExists = disableExists;
        _disableWrites = disableWrites;
        _collection = db.GetCollection<PageMarker>();
    }
        
    public Task<bool> ExistsAsync(Uri uri)
    {
        if (_disableExists)
        {
            return Task.FromResult(false);
        }

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