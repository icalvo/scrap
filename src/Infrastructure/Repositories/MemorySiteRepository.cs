using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Infrastructure.Repositories;

public class MemorySiteRepository : ISiteRepository
{
    private readonly ILogger<MemorySiteRepository> _logger;
    private readonly AsyncLazy<ImmutableDictionary<string, Site>> _store;

    public MemorySiteRepository(IEnumerable<Site> sites, ILogger<MemorySiteRepository> logger)
    {
        _logger = logger;
        _store = AsyncLazy.Create(sites.ToImmutableDictionary(def => def.Name));
    }

    public MemorySiteRepository(
        string jsonFilePath,
        IFileSystemFactory fileSystemFactory,
        ILogger<MemorySiteRepository> logger)
    {
        _logger = logger;
        _store = AsyncLazy.Create(
            async () =>
            {
                var fileSystem = await fileSystemFactory.BuildAsync(false);
                if (!await fileSystem.File.ExistsAsync(jsonFilePath))
                {
                    await fileSystem.File.WriteAllTextAsync(jsonFilePath, "[]");
                }

                await using var stream = await fileSystem.File.OpenReadAsync(jsonFilePath);
                var siteDtos = await JsonSerializer.DeserializeAsync<IEnumerable<SiteDataObject>>(
                    stream,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        Converters =
                        {
                            new ResourceRepositoryConfigurationJsonConverter(),
                            new TimeSpanJsonConverter(),
                            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                        }
                    }) ?? Array.Empty<SiteDataObject>();
                return siteDtos.Select(
                    x => new Site(
                        x.Name,
                        x.ResourceType,
                        x.RootUrl.TryBuildUri(),
                        x.AdjacencyXPath ?? (XPath?)null,
                        x.ResourceXPath ?? (XPath?)null,
                        x.ResourceRepository,
                        x.HttpRequestRetries,
                        x.HttpRequestDelayBetweenRetries,
                        x.UrlPattern)).ToImmutableDictionary(def => def.Name);
            });
    }

    public async Task<Maybe<Site>> GetByNameAsync(string siteName)
    {
        var store = await _store.ValueAsync();
        return store.TryGetValue(siteName, out var result) ? Maybe.Just(result) : Maybe.Nothing<Site>();
    }

    public async IAsyncEnumerable<Site> FindByRootUrlAsync(string rootUrl)
    {
        var store = await _store.ValueAsync();
        var result = store.Values.Where(x => x.UrlPattern != null && Regex.IsMatch(rootUrl, x.UrlPattern));
        foreach (var site in result)
        {
            _logger.LogTrace("Found site {Site} matching URL pattern {UrlPattern}", site.Name, site.UrlPattern);
            yield return site;
        }
    }

    public async IAsyncEnumerable<Site> ListAsync()
    {
        var store = await _store.ValueAsync();
        var result = store.Values;
        foreach (var definition in result)
        {
            yield return definition;
        }
    }

    public IAsyncEnumerable<Site> GetScrappableAsync() =>
        ListAsync().Where(x => x.RootUrl != null && x.HasResourceCapabilities());
}
