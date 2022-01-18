using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Scrap.JobDefinitions.JsonFile;

public class MemoryJobDefinitionRepository: IJobDefinitionRepository
{
    private readonly ImmutableDictionary<string, JobDefinition> _store;

    public MemoryJobDefinitionRepository(IEnumerable<JobDefinition> jobDefinitions)
    {
        _store = jobDefinitions.ToImmutableDictionary(def => def.Name);
    }

    public static async Task<MemoryJobDefinitionRepository> FromJsonFileAsync(string jsonFilePath)
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
                                    })
                                ?? Array.Empty<JobDefinitionDto>();
        return new MemoryJobDefinitionRepository(
            jobDefinitionDtos.Select(x => new JobDefinition(x)));
    }

    public Task<JobDefinition?> GetByIdAsync(JobDefinitionId id)
    {
        return Task.FromResult((JobDefinition?)null);
    }

    public Task<JobDefinition?> GetByNameAsync(string jobName)
    {
        _ = _store.TryGetValue(jobName, out var result);
        return Task.FromResult(result);
    }

    public IAsyncEnumerable<JobDefinition> FindByRootUrlAsync(string rootUrl)
    {
        return _store.Values.Where(x => x.UrlPattern != null && Regex.IsMatch(rootUrl, x.UrlPattern)).ToAsyncEnumerable();
    }

    public Task UpsertAsync(JobDefinition jobDefinition)
    {
        return Task.CompletedTask;
    }

    public Task DeleteJobAsync(JobDefinitionId id)
    {
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<JobDefinition> ListAsync()
    {
        return _store.Values.ToAsyncEnumerable();
    }
}
