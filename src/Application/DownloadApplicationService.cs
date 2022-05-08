using Microsoft.Extensions.Logging;
using Scrap.Domain.Downloads;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application;

public class DownloadApplicationService : IDownloadApplicationService
{
    private readonly IJobFactory _jobFactory;
    private readonly IPageRetriever _pageRetriever;
    private readonly IEnumerable<IResourceRepository> _resourceRepositories;
    private readonly IDownloadStreamProvider _downloadStreamProvider;
    private readonly ILogger<DownloadApplicationService> _logger;

    public DownloadApplicationService(
        IJobFactory jobFactory,
        IPageRetriever pageRetriever,
        IEnumerable<IResourceRepository> resourceRepositories,
        IDownloadStreamProvider downloadStreamProvider,
        ILogger<DownloadApplicationService> logger)
    {
        _jobFactory = jobFactory;
        _pageRetriever = pageRetriever;
        _resourceRepositories = resourceRepositories;
        _downloadStreamProvider = downloadStreamProvider;
        _logger = logger;
    }

    public async Task DownloadAsync(JobDto jobDto, Uri pageUrl, int pageIndex, Uri resourceUrl, int resourceIndex)
    {
        if (jobDto.ResourceType != ResourceType.DownloadLink)
        {
            throw new Exception();
        }

        var job = await _jobFactory.CreateAsync(jobDto);
        var resourceRepository = _resourceRepositories.Single(x => x.Type == job.ResourceRepoArgs.RepositoryType);
        var page = await _pageRetriever.GetPageAsync(pageUrl);

        var info = new ResourceInfo(page, pageIndex, resourceUrl, resourceIndex);
        if (await this.IsNotDownloadedAsync(info, resourceRepository, job.DownloadAlways))
        {
            var stream = await _downloadStreamProvider.GetStreamAsync(resourceUrl);
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
