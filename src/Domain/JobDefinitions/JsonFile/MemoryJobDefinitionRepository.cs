﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Scrap.JobDefinitions.JsonFile
{
    public class MemoryJobDefinitionRepository: IJobDefinitionRepository
    {
        private readonly ImmutableDictionary<string, JobDefinition> _store;

        public MemoryJobDefinitionRepository(IEnumerable<JobDefinition> jobDefinitions)
        {
            _store = jobDefinitions.ToImmutableDictionary(def => def.Name);
        }

        public static async Task<MemoryJobDefinitionRepository> FromJsonFileAsync(string jsonFilePath)
        {
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

        public Task<JobDefinition?> FindByRootUrlAsync(string rootUrl)
        {
            var x = _store.Values.FirstOrDefault(x => x.UrlPattern != null && Regex.IsMatch(rootUrl, x.UrlPattern));
            return Task.FromResult((JobDefinition?)x);
        }

        public Task UpsertAsync(JobDefinition jobDefinition)
        {
            return Task.CompletedTask;
        }

        public Task DeleteJobAsync(JobDefinitionId id)
        {
            return Task.CompletedTask;
        }

        public Task<ImmutableArray<JobDefinition>> ListAsync()
        {
            return Task.FromResult(_store.Values.ToImmutableArray());
        }
    }
}
