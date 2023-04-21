using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scrap.Application;
using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;

namespace Scrap.CommandLine;

internal class JobDtoBuilder : IJobDtoBuilder
{
    private readonly IJobDefinitionsApplicationService _jobDefinitionsApplicationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JobDtoBuilder> _logger;

    public JobDtoBuilder(
        IJobDefinitionsApplicationService jobDefinitionsApplicationService,
        IConfiguration configuration,
        ILogger<JobDtoBuilder> logger)
    {
        _jobDefinitionsApplicationService = jobDefinitionsApplicationService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<JobDto?> BuildJobDtoAsync(
        string? name,
        string? rootUrl,
        bool? fullScan,
        bool? downloadAlways,
        bool? disableMarkingVisited,
        bool? disableResourceWrites)
    {
        Debug.Assert(_configuration != null, nameof(_configuration) + " != null");
        var envName = _configuration.JobDefName();
        var envRootUrl = _configuration.JobDefRootUrl();

        var jobDef = await GetJobDefinitionAsync(name, rootUrl, envName, envRootUrl);

        if (jobDef == null)
        {
            return null;
        }

        _logger.LogInformation("The following job def will be run: {JobDef}", jobDef);

        return new JobDto(
            jobDef,
            rootUrl ?? envRootUrl,
            fullScan,
            null,
            downloadAlways,
            disableMarkingVisited,
            disableResourceWrites);
    }

    public async Task<JobDefinitionDto?> GetJobDefinitionAsync(string? name, string? rootUrl)
    {
        var envRootUrl = _configuration.JobDefRootUrl();
        var envName = _configuration.JobDefName();

        return await GetJobDefinitionAsync(name, rootUrl, envName, envRootUrl);
    }

    public async Task<JobDefinitionDto?> GetJobDefinitionAsync(
        string? name,
        string? rootUrl,
        string? envName,
        string? envRootUrl)
    {
        JobDefinitionDto? jobDef = null;
        if (name != null)
        {
            jobDef = await _jobDefinitionsApplicationService.FindByNameAsync(name);
            if (jobDef == null)
            {
                _logger.LogError("Job definition {Name} does not exist", name);
            }

            return jobDef;
        }

        if (rootUrl != null)
        {
            var jobDefs = await _jobDefinitionsApplicationService.FindByRootUrlAsync(rootUrl).ToArrayAsync();
            if (jobDefs.Length == 0)
            {
                _logger.LogWarning("No job definition matches with {RootUrl}", rootUrl);
            }
            else if (jobDefs.Length > 1)
            {
                _logger.LogWarning("More than one definition matched with {RootUrl}", rootUrl);
            }
            else
            {
                return jobDefs[0];
            }
        }

        if (envName != null)
        {
            jobDef = await _jobDefinitionsApplicationService.FindByNameAsync(envName);
            if (jobDef == null)
            {
                _logger.LogError("Job definition {Name} does not exist", envName);
            }

            return jobDef;
        }

        if (envRootUrl != null)
        {
            var jobDefs = await _jobDefinitionsApplicationService.FindByRootUrlAsync(envRootUrl).ToArrayAsync();
            if (jobDefs.Length == 0)
            {
                _logger.LogWarning("No job definition matches with {RootUrl}", envRootUrl);
            }
            else if (jobDefs.Length > 1)
            {
                _logger.LogWarning("More than one definition matched with {RootUrl}", envRootUrl);
            }
            else
            {
                return jobDefs[0];
            }
        }

        if (jobDef == null)
        {
            _logger.LogWarning("No single job definition was found, nothing will be done");
        }

        return jobDef;
    }
}
