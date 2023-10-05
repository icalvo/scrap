using System.Text.RegularExpressions;
using LiteDB;
using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Domain.Pages;

namespace Scrap.Infrastructure.Repositories;

public class LiteDbVisitedPageRepository : IVisitedPageRepository
{
    private readonly ILiteCollection<VisitedPage> _collection;
    private readonly bool _disableWrites;
    private readonly ILogger<LiteDbVisitedPageRepository> _logger;

    public LiteDbVisitedPageRepository(
        string connectionString,
        ILogger<LiteDbVisitedPageRepository> logger,
        bool disableWrites)
    {
        var cnx = new ConnectionString(connectionString);
        var liteDatabase = new LiteDatabase(cnx);
        _collection = liteDatabase.GetCollection<VisitedPage>();
        _logger = logger;
        _disableWrites = disableWrites;
    }

    public Task<bool> ExistsAsync(Uri uri)
    {
        var findById = _collection.FindById(uri.AbsoluteUri);
        return Task.FromResult(findById != null);
    }

    public IAsyncEnumerable<VisitedPage> GetAllAsync() => _collection.FindAll().ToAsyncEnumerable();

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
                    () => _collection.Upsert(link.AbsoluteUri, new VisitedPage(link.AbsoluteUri)));
            _logger.LogTrace("Inserted marker {Page}", link.AbsoluteUri);
        }
        else
        {
            _logger.LogTrace("FAKE. Inserted marker {Page}", link.AbsoluteUri);
        }

        return Task.CompletedTask;
    }

    public IAsyncEnumerable<VisitedPage> SearchAsync(string search) =>
        _collection.Find(x => Regex.IsMatch(x.Uri, search)).ToAsyncEnumerable();

    public Task DeleteAsync(string search) =>
        Task.FromResult(_collection.DeleteMany(x => Regex.IsMatch(x.Uri, search)));
}
