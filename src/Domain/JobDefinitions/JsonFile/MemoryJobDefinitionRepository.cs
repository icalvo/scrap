using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Scrap.Domain.JobDefinitions.JsonFile;

public class MemoryJobDefinitionRepository : IJobDefinitionRepository
{
    private readonly AsyncLazy<ImmutableDictionary<string, JobDefinition>> _store;
    private readonly ILogger<MemoryJobDefinitionRepository> _logger;

    public MemoryJobDefinitionRepository(
        IEnumerable<JobDefinition> jobDefinitions,
        ILogger<MemoryJobDefinitionRepository> logger)
    {
        _logger = logger;
        _store = AsyncLazy.Create(jobDefinitions.ToImmutableDictionary(def => def.Name));
    }

    public MemoryJobDefinitionRepository(string jsonFilePath, ILogger<MemoryJobDefinitionRepository> logger)
    {
        _logger = logger;
        _store = AsyncLazy.Create(
            async () =>
            {
                if (!File.Exists(jsonFilePath))
                {
                    await File.WriteAllTextAsync(jsonFilePath, "[]");
                }

                await using var stream = File.OpenRead(jsonFilePath);
                var jobDefinitionDtos = await JsonSerializer.DeserializeAsync<IEnumerable<JobDefinitionDto>>(
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
                    }) ?? Array.Empty<JobDefinitionDto>();
                return jobDefinitionDtos.Select(x => new JobDefinition(x)).ToImmutableDictionary(def => def.Name);
            });
    }

    public Task<JobDefinition?> GetByIdAsync(JobDefinitionId id) => Task.FromResult((JobDefinition?)null);

    public async Task<JobDefinition?> GetByNameAsync(string jobName)
    {
        var store = await _store.ValueAsync();
        _ = store.TryGetValue(jobName, out var result);
        return result;
    }

    public async IAsyncEnumerable<JobDefinition> FindByRootUrlAsync(string rootUrl)
    {
        var store = await _store.ValueAsync();
        var result = store.Values.Where(x => x.UrlPattern != null && Regex.IsMatch(rootUrl, x.UrlPattern));
        foreach (var definition in result)
        {
            _logger.LogTrace(
                "Found job def {Definition} matching URL pattern {UrlPattern}",
                definition.Name,
                definition.UrlPattern);
            yield return definition;
        }
    }

    public Task UpsertAsync(JobDefinition jobDefinition) => Task.CompletedTask;

    public Task DeleteJobAsync(JobDefinitionId id) => Task.CompletedTask;

    public async IAsyncEnumerable<JobDefinition> ListAsync()
    {
        var store = await _store.ValueAsync();
        var result = store.Values;
        foreach (var definition in result)
        {
            yield return definition;
        }
    }
}
