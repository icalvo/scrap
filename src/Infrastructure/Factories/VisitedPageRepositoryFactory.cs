using Microsoft.Extensions.Logging;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Infrastructure.Repositories;

namespace Scrap.Infrastructure.Factories;

public class VisitedPageRepositoryFactory : IVisitedPageRepositoryFactory
{
    private readonly DatabaseInfo _options;
    private readonly ILogger<VisitedPageRepositoryFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private static readonly Dictionary<string, IVisitedPageRepository> Store = new();

    public VisitedPageRepositoryFactory(
        DatabaseInfo options,
        ILogger<VisitedPageRepositoryFactory> logger,
        ILoggerFactory loggerFactory)
    {
        _options = options;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public IVisitedPageRepository Build(IVisitedPageRepositoryOptions options) =>
        Build(_options, options.DisableMarkingVisited);

    public IVisitedPageRepository Build(DatabaseInfo options) => Build(options, false);

    public IVisitedPageRepository Build() => Build(_options, false);

    private IVisitedPageRepository Build(DatabaseInfo options, bool disableMarkingVisited)
    {
        var typedConnectionString =
            options.Database ?? throw new ArgumentException("Database cannot be null", nameof(options));
        if (Store.TryGetValue(typedConnectionString, out var repo))
        {
            return repo;
        }

        _logger.LogInformation("Visited Page DB: {FileSystemType}", options.Database);
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

    private IVisitedPageRepository BuildPostgresql(string connectionString, bool disableMarkingVisited)
    {
        var repo = new PostgresqlVisitedPageRepository(
            connectionString,
            _loggerFactory.CreateLogger<PostgresqlVisitedPageRepository>(),
            disableMarkingVisited);

        repo.EnsureTables();

        return repo;
    }

    private IVisitedPageRepository BuildLiteDb(string connectionString, bool disableMarkingVisited) =>
        new LiteDbVisitedPageRepository(
            connectionString,
            _loggerFactory.CreateLogger<LiteDbVisitedPageRepository>(),
            disableMarkingVisited);
}
