using System.Text.RegularExpressions;
using LiteDB;
using Microsoft.Extensions.Logging;
using Polly;

namespace Scrap.Domain.Pages.LiteDb;

public class LiteDbPageMarkerRepository : IPageMarkerRepository
{
    private readonly ILiteCollection<PageMarker> _collection;
    private readonly bool _disableWrites;
    private readonly ILogger<LiteDbPageMarkerRepository> _logger;

    public LiteDbPageMarkerRepository(
        ILiteCollection<PageMarker> collection,
        ILogger<LiteDbPageMarkerRepository> logger,
        bool disableWrites)
    {
        _logger = logger;
        _disableWrites = disableWrites;
        _collection = collection;
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
            Policy
                .Handle<IOException>()
                .Retry(5,
                    (_, retryNumber) =>
                        _logger.LogWarning("IOException while upserting; retry {RetryNumber}", retryNumber))
                .Execute(() => _collection.Upsert(link.AbsoluteUri, new PageMarker(link.AbsoluteUri)));
            _logger.LogTrace("Inserted marker {Page}", link.AbsoluteUri);
        }
        else
        {
            _logger.LogTrace("FAKE. Inserted marker {Page}", link.AbsoluteUri);
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<PageMarker>> SearchAsync(string search) =>
        Task.FromResult(_collection.Find(x => Regex.IsMatch(x.Uri, search)));

    public Task DeleteAsync(string search) =>
        Task.FromResult(_collection.DeleteMany(x => Regex.IsMatch(x.Uri, search)));
}
