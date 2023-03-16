using Microsoft.Extensions.Logging;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Infrastructure.Repositories;

namespace Scrap.Infrastructure.Factories;

public class PageMarkerRepositoryFactory : IPageMarkerRepositoryFactory
{
    private readonly DatabaseInfo _options;
    private readonly ILoggerFactory _loggerFactory;
    private static readonly Dictionary<string, IPageMarkerRepository> Store = new();

    public PageMarkerRepositoryFactory(
        DatabaseInfo options,
        ILogger<PageMarkerRepositoryFactory> logger,
        ILoggerFactory loggerFactory)
    {
        var typedConnectionString = options.Database ?? throw new ArgumentException("Database cannot be null", nameof(options));
        _options = options;
        _loggerFactory = loggerFactory;
        logger.LogDebug("Scrap DB: {ConnectionString}", typedConnectionString);
    }
    
    public IPageMarkerRepository Build(Job job)
    {
        return Build(_options, new JobId(), job.DisableMarkingVisited);
    }

    public IPageMarkerRepository Build(DatabaseInfo param) => Build(param, new JobId(), false);

    public IPageMarkerRepository Build() => Build(_options, new JobId(), false);

    private IPageMarkerRepository Build(DatabaseInfo options, JobId id, bool disableMarkingVisited)
    {
        var typedConnectionString = options.Database ?? throw new ArgumentException("Database cannot be null", nameof(options));
        if (Store.TryGetValue(id.ToString(), out var repo))
            return repo;

        string type;
        string connectionString;
        if (typedConnectionString.StartsWith("["))
        {
            var typeEnd = typedConnectionString.IndexOf("]", StringComparison.Ordinal);
            if (typeEnd == 1)
            {
                throw new Exception("Typed connection string is invalid (no ] found)");
            }

            type = typedConnectionString[1..typeEnd];
            var cnxstrStart = typeEnd + 1;
            connectionString = typedConnectionString[cnxstrStart..];
        }
        else
        {
            type = "LiteDb";
            connectionString = typedConnectionString;
        }

        var instance = type.ToLowerInvariant() switch
        {
            "litedb" => BuildLiteDb(connectionString, disableMarkingVisited),
            "postgres" => BuildPostgres(connectionString, disableMarkingVisited),
            _ => throw new InvalidOperationException($"Database type {type} is not supported")
        };

        Store[id.ToString()] = instance;
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
