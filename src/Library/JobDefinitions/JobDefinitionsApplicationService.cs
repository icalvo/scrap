using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scrap.Resources;

namespace Scrap.JobDefinitions
{
    public class JobDefinitionsApplicationService
    {
        private readonly IJobDefinitionRepository _definitionRepository;
        private readonly IResourceRepositoryFactory _resourceRepositoryFactory;
        private readonly ILogger<JobDefinitionsApplicationService> _logger;

        public JobDefinitionsApplicationService(
            IJobDefinitionRepository definitionRepository,
            ILogger<JobDefinitionsApplicationService> logger,
            IResourceRepositoryFactory resourceRepositoryFactory)
        {
            _definitionRepository = definitionRepository;
            _logger = logger;
            _resourceRepositoryFactory = resourceRepositoryFactory;
        }

        public Task AddJobAsync(string name, JobDefinition jobDefinition)
        {
            _ = _resourceRepositoryFactory.Build(
                jobDefinition.ResourceRepoType,
                jobDefinition.ResourceRepoArgs.C(false.ToString()).ToArray());

            return _definitionRepository.AddAsync(name, jobDefinition);
        }
        

        public Task<JobDefinition> GetJobAsync(string name)
        {
            return _definitionRepository.GetByNameAsync(name);
        }

        public Task<JobDefinition> FindJobByRootUrlAsync(string rootUrl)
        {
            return _definitionRepository.FindJobByRootUrlAsync(rootUrl);
        }

        public Task DeleteJobAsync(string name)
        {
            return _definitionRepository.DeleteJobAsync(name);
        }
    }
}