using Microsoft.Extensions.Logging;
using Scrap.Domain.JobDefinitions;

namespace Scrap.Application;

public class JobDefinitionsApplicationService
{
    private readonly IJobDefinitionRepository _definitionRepository;
    private readonly ILogger<JobDefinitionsApplicationService> _logger;

    public JobDefinitionsApplicationService(
        IJobDefinitionRepository definitionRepository,
        ILogger<JobDefinitionsApplicationService> logger)
    {
        _definitionRepository = definitionRepository;
        _logger = logger;
    }

    public async Task<JobDefinitionDto?> FindJobByNameAsync(string name)
    {
        _logger.LogDebug("Getting job def. called {JobName}", name);
        return (await _definitionRepository.GetByNameAsync(name))?.ToDto();
    }

    public IAsyncEnumerable<JobDefinitionDto> GetJobsAsync()
    {
        _logger.LogDebug("Getting all job defs");
        return _definitionRepository.ListAsync().Select(x => x.ToDto());
    }

    public IAsyncEnumerable<JobDefinitionDto> FindJobsByRootUrlAsync(string rootUrl) =>
        _definitionRepository.FindByRootUrlAsync(rootUrl).Select(x => x.ToDto());

    public Task DeleteJobAsync(JobDefinitionId id)
    {
        _logger.LogDebug("Deleting job def. {JobId}", id);
        return _definitionRepository.DeleteJobAsync(id);
    }
}
