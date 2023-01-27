using Microsoft.Extensions.Logging;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application;

public class DownloadApplicationService : IDownloadApplicationService
{
    private readonly IAsyncFactory<JobDto, Job> _jobFactory;
    private readonly IFactory<Job, IPageRetriever> _pageRetrieverFactory;
    private readonly IFactory<Job, IResourceRepository> _resourceRepositoryFactory;
    private readonly IFactory<Job, IDownloadStreamProvider> _downloadStreamProviderFactory;
    private readonly ILogger<DownloadApplicationService> _logger;

    public DownloadApplicationService(
        IAsyncFactory<JobDto, Job> jobFactory,
        IFactory<Job, IPageRetriever> pageRetrieverFactory,
        IFactory<Job, IResourceRepository> resourceRepositoryFactory,
        IFactory<Job, IDownloadStreamProvider> downloadStreamProviderFactory,
        ILogger<DownloadApplicationService> logger)
    {
        _jobFactory = jobFactory;
        _pageRetrieverFactory = pageRetrieverFactory;
        _resourceRepositoryFactory = resourceRepositoryFactory;
        _downloadStreamProviderFactory = downloadStreamProviderFactory;
        _logger = logger;
    }

    public async Task DownloadAsync(JobDto jobDto, Uri pageUrl, int pageIndex, Uri resourceUrl, int resourceIndex)
    {
        if (jobDto.ResourceType != ResourceType.DownloadLink)
        {
            throw new Exception();
        }

        var job = await _jobFactory.Build(jobDto);
        var resourceRepository = _resourceRepositoryFactory.Build(job);
        var pageRetriever = _pageRetrieverFactory.Build(job);
        var page = await pageRetriever.GetPageAsync(pageUrl);

        var info = new ResourceInfo(page, pageIndex, resourceUrl, resourceIndex);
        if (await this.IsNotDownloadedAsync(info, resourceRepository, job.DownloadAlways))
        {
            var downloadStreamProvider = _downloadStreamProviderFactory.Build(job);
            var stream = await downloadStreamProvider.GetStreamAsync(resourceUrl);
            await resourceRepository.UpsertAsync(info, stream);
            _logger.LogInformation("Downloaded {Url} to {Key}", info.ResourceUrl, await resourceRepository.GetKeyAsync(info));
        }
    }
  
    private async ValueTask<bool> IsNotDownloadedAsync(ResourceInfo info, IResourceRepository resourceRepository, bool downloadAlways)
    {
        if (downloadAlways)
        {
            return true;
        }

        var exists = await resourceRepository.ExistsAsync(info);
        if (!exists)
        {
            return true;
        }

        var key = await resourceRepository.GetKeyAsync(info);
        _logger.LogDebug("{Resource} already downloaded", key);

        return false;
    }
  
}
