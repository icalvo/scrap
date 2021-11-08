using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;
using Scrap.Pages;
using Scrap.Resources;

namespace Scrap
{
    public class ScrapperApplicationService
    {
        private readonly Func<Uri, Func<Uri, Task<Page>>, Func<Page, IAsyncEnumerable<Uri>>, IAsyncEnumerable<Page>> _searchFunc;
        private readonly PageRetrieverFactory _pageRetrieverFactory;
        private readonly IResourceRepositoryFactory _resourceRepositoryFactory;
        private readonly ILogger<ScrapperApplicationService> _logger;
        private readonly PageMarkerRepositoryFactory _pageMarkerRepositoryFactory;
        private readonly HttpPolicyFactory _httpPolicyFactory;
        private readonly IJobDefinitionRepository _definitionRepository;

        public ScrapperApplicationService(
            Func<Uri, Func<Uri, Task<Page>>, Func<Page, IAsyncEnumerable<Uri>>, IAsyncEnumerable<Page>> searchFunc,
            PageRetrieverFactory pageRetrieverFactory,
            IResourceRepositoryFactory resourceRepositoryFactory,
            ILogger<ScrapperApplicationService> logger,
            PageMarkerRepositoryFactory pageMarkerRepositoryFactory,
            HttpPolicyFactory httpPolicyFactory,
            IJobDefinitionRepository definitionRepository)
        {
            _searchFunc = searchFunc;
            _pageRetrieverFactory = pageRetrieverFactory;
            _resourceRepositoryFactory = resourceRepositoryFactory;
            _logger = logger;
            _pageMarkerRepositoryFactory = pageMarkerRepositoryFactory;
            _httpPolicyFactory = httpPolicyFactory;
            _definitionRepository = definitionRepository;
        }

        public async Task ScrapAsync(string? name, string? rootUrl, bool? fullScan, bool? whatIf)
        {
            JobDefinitionDto? scrapJobDefinition;
            if (name == null)
            {
                if (rootUrl == null)
                {
                    throw new Exception("Neither Root URL or a name was provided");
                }

                scrapJobDefinition = (await _definitionRepository.FindJobByRootUrlAsync(rootUrl))?.ToDto();
            }
            else
            {
                scrapJobDefinition = (await _definitionRepository.GetByNameAsync(name))?.ToDto();
                if (scrapJobDefinition != null)
                {
                    scrapJobDefinition = scrapJobDefinition with { RootUrl = rootUrl ?? scrapJobDefinition.RootUrl };
            
                    if (scrapJobDefinition.RootUrl == null)
                    {
                        throw new Exception("No Root URL found as argument or in the job definition");
                    }
                }
            }

            if (scrapJobDefinition == null)
            {
                throw new KeyNotFoundException("No job definition was found based on name or Root URL");
            }

            scrapJobDefinition = scrapJobDefinition with
            {
                RootUrl = rootUrl ?? scrapJobDefinition.RootUrl,
                WhatIf = whatIf ?? scrapJobDefinition.WhatIf,
                FullScan = fullScan ?? scrapJobDefinition.FullScan
            };

            await ScrapAsync(scrapJobDefinition);
        }

        public async Task ScrapAsync(JobDefinitionDto jobDefinitionDto)
        {
            var jobDefinition = new JobDefinition(jobDefinitionDto);
            var (rootUrl, adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute, resourceRepoArgs) =
                (jobDefinition.RootUrl, jobDefinition.AdjacencyXPath, jobDefinition.AdjacencyAttribute, jobDefinition.ResourceXPath, jobDefinition.ResourceAttribute, jobDefinition.ResourceRepoArgs);

            jobDefinition.Log(_logger);
            
            var rootUri = new Uri(rootUrl ?? throw new InvalidOperationException("Root URL must not be null"));
            var baseUrl = new Uri(rootUri.Scheme + "://" + rootUri.Host);
            var pageMarkerRepository = _pageMarkerRepositoryFactory.Build(jobDefinition.FullScan);

            async IAsyncEnumerable<Uri> AdjacencyFunction(Page page)
            {
                foreach (var link in page.Links(adjacencyXPath, adjacencyAttribute, baseUrl))
                {
                    if (await pageMarkerRepository.ExistsAsync(link))
                    {
                        _logger.LogTrace("Page {Link} already visited", link);
                        continue;
                    }

                    await pageMarkerRepository.AddAsync(link);
                    yield return link;
                }
            }

            var httpPolicy = _httpPolicyFactory.Build(jobDefinition.HttpRequestRetries,
                jobDefinition.HttpRequestDelayBetweenRetries);
            var pageRetriever = _pageRetrieverFactory.Build(httpPolicy);
            var pages = _searchFunc(rootUri, pageRetriever.GetPageAsync, AdjacencyFunction);
            
            var resourceRepository = _resourceRepositoryFactory.Build(httpPolicy, resourceRepoArgs, jobDefinition.WhatIf);

            await foreach (var page in pages)
            {
                var resources = page.Links(resourceXPath, resourceAttribute, baseUrl).ToArray();
                if (!resources.Any())
                {
                    _logger.LogInformation("No resources in this page");
                    continue;
                }

                foreach (var (resource, idx) in resources.Select((res, idx) => (res, idx)))
                {
                    await resourceRepository.UpsertResourceAsync(resource, page, idx);
                }
            }
            
            _logger.LogInformation("Finished!");
        }
    }
}