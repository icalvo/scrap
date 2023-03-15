using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application;

public class DownloadApplicationService : IDownloadApplicationService
{
    private readonly IJobFactory _jobFactory;
    private readonly IDownloadStreamProviderFactory _downloadStreamProviderFactory;
    private readonly IPageRetrieverFactory _pageRetrieverFactory;
    private readonly IResourceRepositoryFactory _resourceRepositoryFactory;
    private readonly ILogger<DownloadApplicationService> _logger;

    public DownloadApplicationService(
        IJobFactory jobFactory,
        IPageRetrieverFactory pageRetrieverFactory,
        IResourceRepositoryFactory resourceRepositoryFactory,
        IDownloadStreamProviderFactory downloadStreamProviderFactory,
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

        var job = await _jobFactory.BuildAsync(jobDto);
        var resourceRepository = await _resourceRepositoryFactory.BuildAsync(job);
        var pageRetriever = _pageRetrieverFactory.Build(job);
        var page = await pageRetriever.GetPageAsync(pageUrl);

        var info = new ResourceInfo(page, pageIndex, resourceUrl, resourceIndex);
        if (await IsNotDownloadedAsync(info, resourceRepository, job.DownloadAlways))
        {
            var downloadStreamProvider = _downloadStreamProviderFactory.Build(job);
            var stream = await downloadStreamProvider.GetStreamAsync(resourceUrl);
            await resourceRepository.UpsertAsync(info, stream);
            _logger.LogInformation(
                "Downloaded {Url} to {Key}",
                info.ResourceUrl,
                await resourceRepository.GetKeyAsync(info));
        }
    }

    private async ValueTask<bool> IsNotDownloadedAsync(
        ResourceInfo info,
        IResourceRepository resourceRepository,
        bool downloadAlways)
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
