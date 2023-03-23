using Microsoft.Extensions.Logging;
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
        return Build(_options, job.DisableMarkingVisited);
    }

    public IPageMarkerRepository Build(DatabaseInfo param) => Build(param, false);

    public IPageMarkerRepository Build() => Build(_options, false);

    private IPageMarkerRepository Build(DatabaseInfo options, bool disableMarkingVisited)
    {
        var typedConnectionString = options.Database ?? throw new ArgumentException("Database cannot be null", nameof(options));
        if (Store.TryGetValue(typedConnectionString, out var repo))
            return repo;

        var (type, connectionString) = ParseTypedConnectionString(typedConnectionString);

        var instance = type.ToLowerInvariant() switch
        {
            "litedb" => BuildLiteDb(connectionString, disableMarkingVisited),
            "postgresql" => BuildPostgresql(connectionString, disableMarkingVisited),
            _ => throw new InvalidOperationException($"Database type {type} is not supported")
        };

        Store[typedConnectionString] = instance;
        return instance;
    }

    private static (string type, string connectionString) ParseTypedConnectionString(string typedConnectionString)
    {
        string type;
        string connectionString;
        if (typedConnectionString.StartsWith("["))
        {
            var typeEnd = typedConnectionString.IndexOf("]", StringComparison.Ordinal);
            if (typeEnd == -1)
            {
                throw new Exception("Typed connection string is invalid (no ] found)");
            }

            type = typedConnectionString[1..typeEnd];
            var connectionStringStart = typeEnd + 1;
            connectionString = typedConnectionString[connectionStringStart..];
        }
        else
        {
            type = "LiteDb";
            connectionString = typedConnectionString;
        }

        return (type, connectionString);
    }

    private IPageMarkerRepository BuildPostgresql(string connectionString, bool disableMarkingVisited)
    {
        var repo = new PostgresqlPageMarkerRepository(
            connectionString,
            _loggerFactory.CreateLogger<PostgresqlPageMarkerRepository>(),
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
