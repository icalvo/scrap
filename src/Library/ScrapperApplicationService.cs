using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;
using Scrap.Pages;
using Scrap.Resources;
using Scrap.Resources.FileSystem.Extensions;

namespace Scrap
{
    public class ScrapperApplicationService
    {
        private readonly Func<Uri, Func<Uri, Page>, Func<Page, IEnumerable<Uri>>, IEnumerable<Page>> _searchFunc;
        private readonly IPageRetriever _pageRetriever;
        private readonly IJobDefinitionRepository _definitionRepository;
        private readonly IResourceRepositoryFactory _resourceRepositoryFactory;
        private readonly ILogger<ScrapperApplicationService> _logger;

        public ScrapperApplicationService(
            Func<Uri, Func<Uri, Page>, Func<Page, IEnumerable<Uri>>, IEnumerable<Page>> searchFunc,
            IPageRetriever pageRetriever,
            IJobDefinitionRepository definitionRepository,
            IResourceRepositoryFactory resourceRepositoryFactory,
            ILogger<ScrapperApplicationService> logger)
        {
            _searchFunc = searchFunc;
            _pageRetriever = pageRetriever;
            _definitionRepository = definitionRepository;
            _resourceRepositoryFactory = resourceRepositoryFactory;
            _logger = logger;
        }

        public async Task ScrapAsync(
            JobDefinition jobDefinition,
            bool whatIf = true)
        {
            var (rootUrl, adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute, resourceRepoType, resourceRepoArgs) =
                (jobDefinition.RootUrl, jobDefinition.AdjacencyXPath, jobDefinition.AdjacencyAttribute, jobDefinition.ResourceXPath, jobDefinition.ResourceAttribute, jobDefinition.ResourceRepoType, jobDefinition.ResourceRepoArgs);

            PrintArguments(jobDefinition);
            
            var rootUri = new Uri(rootUrl ?? throw new InvalidOperationException("Root URL must not be null"));
            var baseUrl = new Uri(rootUri.Scheme + "://" + rootUri.Host);

            IEnumerable<Uri> AdjacencyFunction(Page page) => page.Links(adjacencyXPath, adjacencyAttribute, baseUrl);

            var pages = _searchFunc(rootUri, _pageRetriever.GetPage, AdjacencyFunction);

            var resourceRepository = _resourceRepositoryFactory.Build(
                resourceRepoType,
                resourceRepoArgs.C(whatIf.ToString()).ToArray());

            foreach (var page in pages)
            {
                var resources = page.Links(resourceXPath, resourceAttribute, baseUrl).ToArray();
                if (!resources.Any())
                {
                    _logger.LogInformation("No resources in this page");
                    continue;
                }

                foreach (var resource in resources)
                {
                    await resourceRepository.UpsertResourceAsync(resource, page);
                }
            }
            
            _logger.LogInformation("Finished!");
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

        public async Task ScrapAsync(string jobName, bool whatIf, string? rootUrl)
        {
            var scrapJobDefinition = await _definitionRepository.GetByNameAsync(jobName);
            if (scrapJobDefinition.RootUrl == null)
            {
                if (rootUrl == null)
                {
                    throw new Exception("No Root URL found as argument or in the job definition");
                }

                scrapJobDefinition = new JobDefinition(scrapJobDefinition, rootUrl);
            }
            
            await ScrapAsync(scrapJobDefinition, whatIf);
        }
    }
}