using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Scrap.JobDefinitions
{
    public class JobDefinitionsApplicationService
    {
        private readonly IJobDefinitionRepository _definitionRepository;
        private readonly ILogger<JobDefinitionsApplicationService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public JobDefinitionsApplicationService(
            IJobDefinitionRepository definitionRepository,
            ILogger<JobDefinitionsApplicationService> logger,
            ILoggerFactory loggerFactory)
        {
            _definitionRepository = definitionRepository;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public Task<JobDefinitionDto> AddJobAsync(NewJobDefinitionDto definition)
        {
            return UpsertAuxAsync(null, definition);
        }

        public async Task<JobDefinitionDto> UpsertAsync(string jobName, NewJobDefinitionDto definition)
        {
            var jobDefinition = await _definitionRepository.GetByNameAsync(jobName);

            return await UpsertAuxAsync(jobDefinition, definition);
        }

        public async Task<JobDefinitionDto> UpsertAsync(JobDefinitionId id, NewJobDefinitionDto definition)
        {
            var jobDefinition = await _definitionRepository.GetByIdAsync(id);

            return await UpsertAuxAsync(jobDefinition, definition);
        }

        private async Task<JobDefinitionDto> UpsertAuxAsync(JobDefinition? existingJobDefinition, NewJobDefinitionDto definition)
        {
            JobDefinition jobDefinition;
            if (existingJobDefinition == null)
            {
                jobDefinition = new JobDefinition(definition);
            }
            else
            {
                jobDefinition = existingJobDefinition;
                jobDefinition.SetValues(definition);
            }
            
            jobDefinition.ResourceRepoArgs.Validate(_loggerFactory);

            _logger.LogInformation("Upserting job def. {JobId}, name {JobName}", jobDefinition.Id, jobDefinition.Name);
            await _definitionRepository.UpsertAsync(jobDefinition);
            return jobDefinition.ToDto();
        }

        public async Task<JobDefinitionDto?> GetJobAsync(JobDefinitionId id)
        {
            _logger.LogInformation("Getting job def. {JobId}", id);
            return (await _definitionRepository.GetByIdAsync(id))?.ToDto();
        }

        public async Task<JobDefinitionDto?> FindJobByNameAsync(string name)
        {
            _logger.LogInformation("Getting job def. called {JobName}", name);
            return (await _definitionRepository.GetByNameAsync(name))?.ToDto();
        }

        public IAsyncEnumerable<JobDefinitionDto> GetJobsAsync()
        {
            _logger.LogInformation("Getting all job defs");
            return _definitionRepository.ListAsync().Select(x => x.ToDto());
        }

        public async Task<JobDefinitionDto?> FindJobByRootUrlAsync(string rootUrl)
        {
            var jobDefinition = (await _definitionRepository.FindByRootUrlAsync(rootUrl))?.ToDto();
            if (jobDefinition == null)
            {
                return null;
            }

            _logger.LogInformation("Got job def. by URL {RootUrl}: {Name}", rootUrl, jobDefinition.Name);
            return jobDefinition;
        }

        public Task DeleteJobAsync(JobDefinitionId id)
        {
            _logger.LogInformation("Deleting job def. {JobId}", id);
            return _definitionRepository.DeleteJobAsync(id);
        }
    }
}
