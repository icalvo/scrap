using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;
using Scrap.Resources;
using Scrap.Resources.FileSystem.Extensions;

namespace Scrap
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

        private void PrintArguments(JobDefinition jobDefinition)
        {
            var (rootUrl, adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute, resourceRepoType, resourceRepoArgs) =
                (jobDefinition.RootUrl, jobDefinition.AdjacencyXPath, jobDefinition.AdjacencyAttribute, jobDefinition.ResourceXPath, jobDefinition.ResourceAttribute, jobDefinition.ResourceRepoType, jobDefinition.ResourceRepoArgs);

            _logger.LogDebug("Root URL: {RootUrl}", rootUrl);
            _logger.LogDebug("Adjacency X-Path: {AdjacencyXPath}", adjacencyXPath);
            _logger.LogDebug("Adjacency attribute: {AdjacencyAttribute}", adjacencyAttribute);
            _logger.LogDebug("Resource X-Path: {ResourceXPath}", resourceXPath);
            _logger.LogDebug("Resource attribute: {ResourceAttribute}", resourceAttribute);
            _logger.LogDebug("Resource repo type: {ResourceRepoType}", resourceRepoType);
            _logger.LogDebug("Resource repo args: {ResourceRepoArgs}", string.Join(" , ", resourceRepoArgs));
        }

        public Task AddJobAsync(string name, JobDefinition jobDefinition)
        {
            PrintArguments(jobDefinition);
            _ = _resourceRepositoryFactory.Build(
                jobDefinition.ResourceRepoType,
                jobDefinition.ResourceRepoArgs.C(false.ToString()).ToArray());

            return _definitionRepository.AddAsync(name, jobDefinition);
        }
        

        public Task<JobDefinition> GetJobAsync(string name)
        {
            return _definitionRepository.GetByNameAsync(name);
        }        
    }
}