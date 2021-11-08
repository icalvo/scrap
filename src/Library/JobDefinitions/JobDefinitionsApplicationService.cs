using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scrap.Resources;

namespace Scrap.JobDefinitions
{
    public class JobDefinitionsApplicationService
    {
        private readonly IJobDefinitionRepository _definitionRepository;
        private readonly IResourceRepositoryConfigurationValidator _resourceRepositoryConfigurationValidator;

        public JobDefinitionsApplicationService(
            IJobDefinitionRepository definitionRepository,
            IResourceRepositoryConfigurationValidator resourceRepositoryConfigurationValidator)
        {
            _definitionRepository = definitionRepository;
            _resourceRepositoryConfigurationValidator = resourceRepositoryConfigurationValidator;
        }

        public Task AddJobAsync(string name, JobDefinitionDto jobDefinitionDto)
        {
            var jobDefinition = new JobDefinition(jobDefinitionDto);
            _resourceRepositoryConfigurationValidator.Validate(jobDefinition.ResourceRepoArgs);

            return _definitionRepository.AddAsync(name, jobDefinition);
        }

        public async Task<JobDefinitionDto?> GetJobAsync(string name)
        {
            return (await _definitionRepository.GetByNameAsync(name))?.ToDto();
        }

        public async Task<ImmutableArray<JobDefinitionDto>> GetJobsAsync()
        {
            return (await _definitionRepository.ListAsync()).Select(x => x.ToDto()).ToImmutableArray();
        }

        public async Task<JobDefinitionDto?> FindJobByRootUrlAsync(string rootUrl)
        {
            return (await _definitionRepository.FindJobByRootUrlAsync(rootUrl))?.ToDto();
        }

        public Task DeleteJobAsync(string name)
        {
            return _definitionRepository.DeleteJobAsync(name);
        }
    }
}