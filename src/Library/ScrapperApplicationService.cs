using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IPageMarkerRepository _pageMarkerRepository;
        private readonly HttpPolicyFactory _httpPolicyFactory;

        public ScrapperApplicationService(
            Func<Uri, Func<Uri, Task<Page>>, Func<Page, IAsyncEnumerable<Uri>>, IAsyncEnumerable<Page>> searchFunc,
            PageRetrieverFactory pageRetrieverFactory,
            IResourceRepositoryFactory resourceRepositoryFactory,
            ILogger<ScrapperApplicationService> logger,
            IPageMarkerRepository pageMarkerRepository,
            HttpPolicyFactory httpPolicyFactory)
        {
            _searchFunc = searchFunc;
            _pageRetrieverFactory = pageRetrieverFactory;
            _resourceRepositoryFactory = resourceRepositoryFactory;
            _logger = logger;
            _pageMarkerRepository = pageMarkerRepository;
            _httpPolicyFactory = httpPolicyFactory;
        }

        public async Task ScrapAsync(JobDefinitionDto jobDefinitionDto)
        {
            var jobDefinition = new JobDefinition(jobDefinitionDto);
            var (rootUrl, adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute, resourceRepoArgs) =
                (jobDefinition.RootUrl, jobDefinition.AdjacencyXPath, jobDefinition.AdjacencyAttribute, jobDefinition.ResourceXPath, jobDefinition.ResourceAttribute, jobDefinition.ResourceRepoArgs);

            jobDefinition.Log(_logger);
            
            var rootUri = new Uri(rootUrl ?? throw new InvalidOperationException("Root URL must not be null"));
            var baseUrl = new Uri(rootUri.Scheme + "://" + rootUri.Host);

            async IAsyncEnumerable<Uri> AdjacencyFunction(Page page)
            {

                foreach (var link in page.Links(adjacencyXPath, adjacencyAttribute, baseUrl))
                {
                    if (await _pageMarkerRepository.ExistsAsync(link))
                    {
                        continue;
                    }

                    await _pageMarkerRepository.AddAsync(link);
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