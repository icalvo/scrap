using System.Text.RegularExpressions;
using LiteDB;
using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Domain.Pages;

namespace Scrap.Infrastructure.Repositories;

public class LiteDbPageMarkerRepository : IPageMarkerRepository
{
    private readonly ILiteCollection<PageMarker> _collection;
    private readonly bool _disableWrites;
    private readonly ILogger<LiteDbPageMarkerRepository> _logger;

    public LiteDbPageMarkerRepository(
        string connectionString,
        ILogger<LiteDbPageMarkerRepository> logger,
        bool disableWrites)
    {
        var cnx = new ConnectionString(connectionString);
        var liteDatabase = new LiteDatabase(cnx);
        _collection = liteDatabase.GetCollection<PageMarker>();
        _logger = logger;
        _disableWrites = disableWrites;
    }

    public Task<bool> ExistsAsync(Uri uri)
    {
        var findById = _collection.FindById(uri.AbsoluteUri);
        return Task.FromResult(findById != null);
    }

    public Task<IEnumerable<PageMarker>> GetAllAsync() => Task.FromResult(_collection.FindAll());

    public Task UpsertAsync(Uri link)
    {
        if (!_disableWrites)
        {
            Policy.Handle<IOException>()
                .Retry(
                    5,
                    (_, retryNumber) => _logger.LogWarning(
                        "IOException while upserting; retry {RetryNumber}",
                        retryNumber)).Execute(
                    () => _collection.Upsert(link.AbsoluteUri, new PageMarker(link.AbsoluteUri)));
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
