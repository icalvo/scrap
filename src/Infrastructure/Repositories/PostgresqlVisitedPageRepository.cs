using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Scrap.Domain.Pages;

namespace Scrap.Infrastructure.Repositories;

public class PostgresqlVisitedPageRepository : IVisitedPageRepository
{
    private readonly bool _disableWrites;
    private readonly ILogger<PostgresqlVisitedPageRepository> _logger;
    private readonly NpgsqlConnection _connection;

    public PostgresqlVisitedPageRepository(
        string connectionString,
        ILogger<PostgresqlVisitedPageRepository> logger,
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

    public async IAsyncEnumerable<VisitedPage> GetAllAsync()
    {
        var query = await _connection.QueryAsync<VisitedPage>("SELECT url AS uri FROM page_markers");

        foreach (var item in query)
        {
            yield return item;
        }
    }

    public async Task UpsertAsync(Uri link)
    {
        if (!_disableWrites)
        {
            await Policy.Handle<IOException>()
                .RetryAsync(
                    5,
                    (_, retryNumber) => _logger.LogWarning(
                        "IOException while upserting; retry {RetryNumber}",
                        retryNumber)).ExecuteAsync(
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

    public async IAsyncEnumerable<VisitedPage> SearchAsync(string search)
    {
        var query = await _connection.QueryAsync<VisitedPage>(
            "SELECT url AS uri FROM page_markers WHERE url ~* @search",
            new { search });

        foreach (var item in query)
        {
            yield return item;
        }
    }

    public Task DeleteAsync(string search) =>
        _connection.ExecuteAsync("DELETE FROM page_markers WHERE url ~* @search", new { search });

    public void EnsureTables() =>
        _connection.Execute("CREATE TABLE IF NOT EXISTS public.page_markers (url VARCHAR(512) NOT NULL PRIMARY KEY);");
}
