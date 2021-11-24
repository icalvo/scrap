using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scrap.Downloads;
using Scrap.JobDefinitions;
using Scrap.Jobs;
using Scrap.Jobs.Graphs;
using Scrap.Pages;
using Scrap.Resources;

namespace Scrap
{
    public class JobApplicationService
    {
        private readonly IGraphSearch _graphSearch;
        private readonly ILogger<JobApplicationService> _logger;
        private readonly IJobServicesResolver _servicesResolver;
        private readonly IJobFactory _jobFactory;

        public JobApplicationService(
            IGraphSearch graphSearch,
            IJobServicesResolver servicesResolver,
            IJobFactory jobFactory,
            ILogger<JobApplicationService> logger)
        {
            _graphSearch = graphSearch;
            _logger = logger;
            _servicesResolver = servicesResolver;
            _jobFactory = jobFactory;
        }

        public async Task RunAsync(NewJobDto jobDto)
        {
            _logger.LogTrace("Trace enabled");
            _logger.LogInformation("Starting...");
            await (jobDto.ResourceType switch
            {
                ResourceType.DownloadLink => DownloadLinksAsync(jobDto),
                ResourceType.Text => ScrapTextAsync(jobDto),
                _ => throw new ArgumentOutOfRangeException()
            });
            _logger.LogInformation("Finished!");
        }

        public Task ListResourcesAsync(NewJobDto jobDto)
        {
            var (rootUri, adjacencyXPath, resourceXPath, _, resourceRepository, pageRetriever, pageMarkerRepository) =
                GetJobInfoAndCreateDependencies(jobDto);

            var pipeline =
                Pages(rootUri, pageRetriever, adjacencyXPath, pageMarkerRepository)
                .SelectMany((page, crawlPageIndex) => ResourceLinks(page, crawlPageIndex, resourceXPath))
                .WhereAwait(x => IsNotDownloadedAsync(x, resourceRepository))
                .ForEachAsync(x => _logger.LogWarning("{Uri}", x.ResourceUrl.AbsoluteUri));

            return pipeline;
        }

        private Task DownloadLinksAsync(NewJobDto jobDto)
        {
            var (rootUri, adjacencyXPath, resourceXPath, downloadStreamProvider, resourceRepository, pageRetriever, pageMarkerRepository) =
                GetJobInfoAndCreateDependencies(jobDto);

            async Task Download((ResourceInfo info, Stream stream) x)
            {
                var (info, stream) = x;
                await resourceRepository.UpsertAsync(info, stream);
                _logger.LogInformation("Downloaded {Url} to {Key}", info.ResourceUrl, await resourceRepository.GetKeyAsync(info));
            }

            IAsyncEnumerable<ResourceInfo> GetResourceLinks(Page page, int crawlPageIndex)
                => ResourceLinks(page, crawlPageIndex, resourceXPath);

            ValueTask<bool> IsNotDownloaded(ResourceInfo info)
                => this.IsNotDownloadedAsync(info, resourceRepository);

            var pipeline =
                Pages(rootUri, pageRetriever, adjacencyXPath, pageMarkerRepository)
                    .ForEachAwaitAsync(async (page, pageIndex) =>
                    {
                        _logger.LogInformation("Processing page {PageUrl}", page.Uri);
                        await GetResourceLinks(page, pageIndex)
                            .WhereAwait(IsNotDownloaded)
                            .SelectAwait(async resourceLink => (
                                x: resourceLink,
                                stream: await downloadStreamProvider.GetStreamAsync(resourceLink.ResourceUrl)))
                            .ForEachAwaitAsync(Download);
                        await pageMarkerRepository.UpsertAsync(page.Uri);
                    });

            return pipeline;
        }

        private async Task ScrapTextAsync(NewJobDto jobDto)
        {
            var (rootUri, adjacencyXPath,resourceXPath,_, resourceRepository, pageRetriever, pageMarkerRepository) =
                GetJobInfoAndCreateDependencies(jobDto);
            
            IAsyncEnumerable<(ResourceInfo info, string text)> PageTexts(Page page, int crawlPageIndex) =>
                page.Contents(resourceXPath)
                    .Where(text => text != null)
                    .Select((text, textIndex) => (
                        info: new ResourceInfo(page, crawlPageIndex, page.Uri, textIndex),
                        text: text ?? ""))
                    .ToAsyncEnumerable();

            _logger.LogDebug("Defining pipeline...");
            var pipeline =
                Pages(rootUri, pageRetriever, adjacencyXPath, pageMarkerRepository)
                    .ForEachAwaitAsync(async (page, pageIndex) =>
                    {
                        await PageTexts(page, pageIndex)
                            .WhereAwait(x => IsNotDownloadedAsync(x.info, resourceRepository))
                            .Select(x => (
                                x.info,
                                stream: (Stream)new MemoryStream(Encoding.UTF8.GetBytes(x.text))))
                            .ForEachAwaitAsync(async y =>
                            {
                                var (info, stream) = y;
                                await resourceRepository.UpsertAsync(info, stream);
                                _logger.LogInformation("Downloaded text from {Url} to {Key}", info.ResourceUrl, await resourceRepository.GetKeyAsync(info));
                            });
                        await pageMarkerRepository.UpsertAsync(page.Uri);
                    });

            await pipeline;
            _logger.LogInformation("Finished!");
        }

        private (
            Uri rootUri,
            XPath? adjacencyXPath,
            XPath resourceXPath,
            IDownloadStreamProvider downloadStreamProvider,
            IResourceRepository resourceRepository,
            IPageRetriever pageRetriever,
            IPageMarkerRepository pageMarkerRepository) GetJobInfoAndCreateDependencies(NewJobDto jobDto)
        {
            var job = _jobFactory.Create(jobDto);

            var (rootUri, adjacencyXPath, resourceXPath) =
                (job.RootUrl, job.AdjacencyXPath, job.ResourceXPath);

            job.Log(_logger);

            _logger.LogDebug("Building job-specific dependencies...");
            var (downloadStreamProvider, resourceRepository, pageRetriever, pageMarkerRepository) = _servicesResolver.BuildJobDependencies(job);
            
            return (rootUri, adjacencyXPath, resourceXPath, downloadStreamProvider, resourceRepository, pageRetriever, pageMarkerRepository);
        }

        private static IAsyncEnumerable<ResourceInfo> ResourceLinks(
            Page page, int crawlPageIndex, XPath resourceXPathExpression)
        {
            var links = page.Links(resourceXPathExpression).ToArray();
            return links.Select((resourceUrl, resourceIndex) => new ResourceInfo(page, crawlPageIndex, resourceUrl, resourceIndex))
                .ToAsyncEnumerable();
        }

        private async IAsyncEnumerable<Uri> CalculateLinks(
            Page page,
            XPath? adjacencyXPath,
            IPageMarkerRepository pageMarkerRepository)
        {
            if (adjacencyXPath == null)
            {
                yield break;
            }

            var links = page.Links(adjacencyXPath).ToArray();
            if (links.Length == 0)
            {
                _logger.LogTrace("No links at {PageUri}", page.Uri);
                yield break;
            }

            foreach (var link in links)
            {
                if (await pageMarkerRepository.ExistsAsync(link))
                {
                    _logger.LogTrace("Page {Link} already visited", link);
                    continue;
                }

                yield return link;
            }
        }
        private IAsyncEnumerable<Page> Pages(
            Uri rootUri,
            IPageRetriever pageRetriever,
            XPath? adjacencyXPath,
            IPageMarkerRepository pageMarkerRepository)
        {
            return _graphSearch.SearchAsync(
                rootUri,
                pageRetriever.GetPageAsync,
                page => CalculateLinks(page, adjacencyXPath, pageMarkerRepository));
        }

        private async ValueTask<bool> IsNotDownloadedAsync(ResourceInfo info, IResourceRepository resourceRepository)
        {
            var exists = await resourceRepository.ExistsAsync(info);
            if (exists)
            {
                var key = await resourceRepository.GetKeyAsync(info);
                _logger.LogInformation("{Resource} already downloaded", key);
            }

            return !exists;
        }
    }
}
