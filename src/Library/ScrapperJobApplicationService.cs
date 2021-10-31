using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;

namespace Scrap
{
    public class ScrapperJobApplicationService
    {
        private readonly IJobDefinitionRepository _definitionRepository;
        private readonly ILogger<ScrapperJobApplicationService> _logger;

        public ScrapperJobApplicationService(
            IJobDefinitionRepository definitionRepository,
            ILogger<ScrapperJobApplicationService> logger)
        {
            _definitionRepository = definitionRepository;
            _logger = logger;
        }

        private void PrintArguments(ScrapJobDefinition scrapJobDefinition)
        {
            var (rootUrl, adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute, destinationRootFolder, destinationExpression) =
                (scrapJobDefinition.RootUrl, scrapJobDefinition.AdjacencyXPath, scrapJobDefinition.AdjacencyAttribute, scrapJobDefinition.ResourceXPath, scrapJobDefinition.ResourceAttribute, scrapJobDefinition.DestinationRootFolder, scrapJobDefinition.DestinationExpression);

            _logger.LogDebug("Root URL: {RootUrl}", rootUrl);
            _logger.LogDebug("Adjacency X-Path: {AdjacencyXPath}", adjacencyXPath);
            _logger.LogDebug("Adjacency attribute: {AdjacencyAttribute}", adjacencyAttribute);
            _logger.LogDebug("Resource X-Path: {ResourceXPath}", resourceXPath);
            _logger.LogDebug("Resource attribute: {ResourceAttribute}", resourceAttribute);
            _logger.LogDebug("Destination root folder: {DestinationRootFolder}", destinationRootFolder);
            _logger.LogDebug("Destination expression: {DestinationExpression}", destinationExpression);
        }

        public Task AddJob(string name, ScrapJobDefinition scrapJobDefinition)
        {
            PrintArguments(scrapJobDefinition);
            return _definitionRepository.AddAsync(name, scrapJobDefinition);
        }
    }
}