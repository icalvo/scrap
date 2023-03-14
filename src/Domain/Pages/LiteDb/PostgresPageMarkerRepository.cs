using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;

namespace Scrap.Domain.Pages.LiteDb;

public class PostgresPageMarkerRepository : IPageMarkerRepository
{
    private readonly bool _disableWrites;
    private readonly ILogger<PostgresPageMarkerRepository> _logger;
    private readonly NpgsqlConnection _connection;

    public PostgresPageMarkerRepository(
        string connectionString,
        ILogger<PostgresPageMarkerRepository> logger,
        bool disableWrites)
    {
        _logger = logger;
        _disableWrites = disableWrites;
        _connection = new NpgsqlConnection(connectionString);
    }

    public async Task<bool> ExistsAsync(Uri uri)
    {
        var x = await _connection.QueryAsync(
            "SELECT url FROM page_markers WHERE url = @url",
            new { Url = uri.AbsoluteUri });
        return x.Any();
    }

    public Task<IEnumerable<PageMarker>> GetAllAsync() =>
        _connection.QueryAsync<PageMarker>("SELECT url AS uri FROM page_markers");

    public async Task UpsertAsync(Uri link)
    {
        if (!_disableWrites)
        {
            await Policy.Handle<IOException>()
                .RetryAsync(
                    5,
                    (_, retryNumber) => _logger.LogWarning(
                        "IOException while upserting; retry {RetryNumber}",
                        retryNumber))
                    .ExecuteAsync(
                    () => _connection.ExecuteAsync(
                        "UPSERT INTO page_markers(url) VALUES(@Url)",
                        new { Url = link.AbsoluteUri }));
            _logger.LogTrace("Inserted marker {Page}", link.AbsoluteUri);
        }
        else
        {
            _logger.LogTrace("FAKE. Inserted marker {Page}", link.AbsoluteUri);
        }
    }

    public Task<IEnumerable<PageMarker>> SearchAsync(string search) =>
        _connection.QueryAsync<PageMarker>("SELECT url AS uri FROM page_markers WHERE url ~* @search", new { search });

    public Task DeleteAsync(string search) =>
        _connection.ExecuteAsync("DELETE page_markers WHERE url ~* @search", new { search });

    public void EnsureTables()
    {
        _connection.Execute("CREATE TABLE IF NOT EXISTS public.page_markers (url VARCHAR(512) NOT NULL PRIMARY KEY);");
    }
}
