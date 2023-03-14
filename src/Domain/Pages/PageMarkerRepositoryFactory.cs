using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages.LiteDb;

namespace Scrap.Domain.Pages;

public class PageMarkerRepositoryFactory : ISingleOptionalParameterFactory<Job, IPageMarkerRepository>
{
    private readonly string _typedConnectionString;
    private readonly ILoggerFactory _loggerFactory;
    private static readonly Dictionary<string, IPageMarkerRepository> _store = new();

    public PageMarkerRepositoryFactory(
        DatabaseInfo options,
        ILogger<PageMarkerRepositoryFactory> logger,
        ILoggerFactory loggerFactory)
    {
        _typedConnectionString = options.Database ?? throw new ArgumentException("Database cannot be null", nameof(options));
        _loggerFactory = loggerFactory;
        logger.LogDebug("Scrap DB: {ConnectionString}", _typedConnectionString);
    }
    
    public IPageMarkerRepository Build(Job job)
    {
        return Build(new JobId(), job.DisableMarkingVisited);
    }

    public IPageMarkerRepository Build() => Build(new JobId(), false);

    private IPageMarkerRepository Build(JobId id, bool disableMarkingVisited)
    {
        if (_store.TryGetValue(id.ToString(), out var repo))
            return repo;

        string type;
        string connectionString;
        if (_typedConnectionString.StartsWith("["))
        {
            var typeEnd = _typedConnectionString.IndexOf("]", StringComparison.Ordinal);
            if (typeEnd == 1)
            {
                throw new Exception("Typed connection string is invalid (no ] found)");
            }

            type = _typedConnectionString[1..typeEnd];
            var cnxstrStart = typeEnd + 1;
            connectionString = _typedConnectionString[cnxstrStart..];
        }
        else
        {
            type = "LiteDb";
            connectionString = _typedConnectionString;
        }
        var instance = type.ToLowerInvariant() switch
        {
            "litedb" => BuildLiteDb(connectionString, disableMarkingVisited),
            "postgres" => BuildPostgres(connectionString, disableMarkingVisited),
            _ => throw new InvalidOperationException($"Database type {type} is not supported")
        };

        _store[id.ToString()] = instance;
        return instance;
    }

    private IPageMarkerRepository BuildPostgres(string connectionString, bool disableMarkingVisited)
    {
        var repo = new PostgresPageMarkerRepository(
            connectionString,
            _loggerFactory.CreateLogger<PostgresPageMarkerRepository>(),
            disableMarkingVisited);

        repo.EnsureTables();

        return repo;
    }

    private IPageMarkerRepository BuildLiteDb(string connectionString, bool disableMarkingVisited)
    {
        return new LiteDbPageMarkerRepository(
            connectionString,
            _loggerFactory.CreateLogger<LiteDbPageMarkerRepository>(),
            disableMarkingVisited);
    }
}
