using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scrap.Downloads;
using Scrap.Pages;
using Scrap.Resources;

namespace Scrap.ResourceDownloaders
{
    public class DownloadResourceProcessor<TId>: IResourceProcessor
        where TId: IResourceId
    {
        private readonly ILogger<DownloadResourceProcessor<TId>> _logger;
        private readonly IDownloadStreamProvider _downloadStreamProvider;
        private readonly IResourceRepository<TId> _resourceRepository;
        public DownloadResourceProcessor(
            IDownloadStreamProvider downloadStreamProvider,
            ILogger<DownloadResourceProcessor<TId>> logger,
            IResourceRepository<TId> resourceRepository)
        {
            _downloadStreamProvider = downloadStreamProvider;
            _logger = logger;
            _resourceRepository = resourceRepository;
        }

        public async Task DownloadResourceAsync(
            Page page,
            int pageIndex,
            Uri resourceUrl,
            int resourceIndex)
        {
            TId id = await _resourceRepository.GetIdAsync(page, pageIndex, resourceUrl, resourceIndex);
            if (await _resourceRepository.ExistsAsync(id))
            {
                _logger.LogInformation("{Resource} already downloaded to {Key}", resourceUrl, id);
            }
            else
            {
                await using (var resourceStream = await _downloadStreamProvider.GetStreamAsync(resourceUrl))
                {
                    await _resourceRepository.UpsertAsync(id, resourceStream);
                }

                _logger.LogInformation("{Resource} downloaded to {Key}", resourceUrl, id);
            }
        }            
    }
}