using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly IJobServicesResolver _jobServicesResolver;
        private readonly IJobFactory _jobFactory;

        public JobApplicationService(
            IGraphSearch graphSearch,
            IJobServicesResolver jobServicesResolver,
            IJobFactory jobFactory,
            ILogger<JobApplicationService> logger)
        {
            _graphSearch = graphSearch;
            _logger = logger;
            _jobServicesResolver = jobServicesResolver;
            _jobFactory = jobFactory;
        }

        public async Task ListResourcesAsync(NewJobDto jobDto)
        {
            var job = _jobFactory.Create(jobDto);
            job.Log(_logger);

            var (rootUri, adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute) =
                (job.RootUrl, job.AdjacencyXPath, job.AdjacencyAttribute, job.ResourceXPath, job.ResourceAttribute);

            var (_, resourceRepository, linkedPagesCalculator, pageRetriever) = _jobServicesResolver.Build(job);

            var pipeline =
                Pages(rootUri, pageRetriever, linkedPagesCalculator, adjacencyXPath, adjacencyAttribute)
                .SelectMany((page, crawlPageIndex) => ResourceLinks(page, crawlPageIndex, resourceXPath, resourceAttribute))
                .WhereAwait(x => IsNotDownloadedAsync(x, resourceRepository))
                .ForEachAsync(x => _logger.LogWarning("{Uri}", x.ResourceUrl.AbsoluteUri));

            await pipeline;
            _logger.LogInformation("Finished!");
        }

        public async Task RunAsync(NewJobDto jobDto)
        {
            var job = _jobFactory.Create(jobDto);
            job.Log(_logger);

            var (rootUri, adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute) =
                (job.RootUrl, job.AdjacencyXPath, job.AdjacencyAttribute, job.ResourceXPath, job.ResourceAttribute);

            var (downloadStreamProvider, resourceRepository, linkedPagesCalculator, pageRetriever) = _jobServicesResolver.Build(job);            

            async Task Download((ResourceInfo info, Stream stream) x)
            {
                var (info, stream) = x;
                await resourceRepository.UpsertAsync(info, stream);
            }
            IAsyncEnumerable<ResourceInfo> GetResourceLinks(Page page, int crawlPageIndex)
                => ResourceLinks(page, crawlPageIndex, resourceXPath, resourceAttribute);
            ValueTask<bool> IsNotDownloaded(ResourceInfo info) => this.IsNotDownloadedAsync(info, resourceRepository);

            var pipeline =
                Pages(rootUri, pageRetriever, linkedPagesCalculator, adjacencyXPath, adjacencyAttribute)
                .SelectMany(GetResourceLinks)
                .WhereAwait(IsNotDownloaded)
                .SelectAwait(async resourceLink => (
                    x: resourceLink,
                    stream: await downloadStreamProvider.GetStreamAsync(resourceLink.ResourceUrl)))
                .ForEachAwaitAsync(Download);

            await pipeline;

            _logger.LogInformation("Finished!");
        }

        private static IAsyncEnumerable<ResourceInfo> ResourceLinks(
            Page page, int crawlPageIndex, string resourceXPath, string resourceAttribute)
        {
            var links = page.Links(resourceXPath, resourceAttribute).ToArray();
            return links.Select((resourceUrl, resourceIndex) => new ResourceInfo(page, crawlPageIndex, resourceUrl, resourceIndex))
                .ToAsyncEnumerable();
        }

        public async Task ScrapTextAsync(NewJobDto jobDto)
        {
            var job = _jobFactory.Create(jobDto);

            var (rootUri, adjacencyXPath, adjacencyAttribute, resourceXPath) =
                (job.RootUrl, job.AdjacencyXPath, job.AdjacencyAttribute, job.ResourceXPath);

            job.Log(_logger);
            var (_, resourceRepository, linkedPagesCalculator, pageRetriever) = _jobServicesResolver.Build(job);            

            IAsyncEnumerable<(ResourceInfo info, string text)> PageTexts(Page page, int crawlPageIndex) =>
                page.Texts(resourceXPath)
                    .Where(text => text != null)
                    .Select((text, textIndex) => (
                        info: new ResourceInfo(page, crawlPageIndex, page.Uri, textIndex),
                        text: text ?? ""))
                    .ToAsyncEnumerable();

            var pipeline =
                Pages(rootUri, pageRetriever, linkedPagesCalculator, adjacencyXPath, adjacencyAttribute)
                .SelectMany(PageTexts)
                .WhereAwait(x => IsNotDownloadedAsync(x.info, resourceRepository))
                .Select(x => (
                    x.info,
                    stream: (Stream)new MemoryStream(Encoding.UTF8.GetBytes(x.text))))
                .ForEachAwaitAsync(async y => await resourceRepository.UpsertAsync(y.info, y.stream));

            await pipeline;

            _logger.LogInformation("Finished!");
        }

        private IAsyncEnumerable<Page> Pages(Uri rootUri, IPageRetriever pageRetriever,
            LinkedPagesCalculator linkedPagesCalculator, string adjacencyXPath, string adjacencyAttribute)
        {
            var pages = _graphSearch.SearchAsync(
                rootUri,
                uri => pageRetriever.GetPageAsync(uri),
                page => linkedPagesCalculator.GetLinkedPagesAsync(page, adjacencyXPath, adjacencyAttribute));
            return pages;
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
