using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources;

namespace Scrap.Application.Download;

public class DownloadApplicationService : IDownloadApplicationService
{
    private readonly IDownloadStreamProviderFactory _downloadStreamProviderFactory;
    private readonly IPageRetrieverFactory _pageRetrieverFactory;
    private readonly IResourceRepositoryFactory _resourceRepositoryFactory;
    private readonly ILogger<DownloadApplicationService> _logger;
    private readonly ICommandJobBuilder<IDownloadCommand, IDownloadJob> _commandJobBuilder;

    public DownloadApplicationService(
        IPageRetrieverFactory pageRetrieverFactory,
        IResourceRepositoryFactory resourceRepositoryFactory,
        IDownloadStreamProviderFactory downloadStreamProviderFactory,
        ILogger<DownloadApplicationService> logger,
        ICommandJobBuilder<IDownloadCommand, IDownloadJob> commandJobBuilder)
    {
        _pageRetrieverFactory = pageRetrieverFactory;
        _resourceRepositoryFactory = resourceRepositoryFactory;
        _downloadStreamProviderFactory = downloadStreamProviderFactory;
        _logger = logger;
        _commandJobBuilder = commandJobBuilder;
    }

    public Task DownloadAsync(IDownloadCommand command) =>
        _commandJobBuilder.Build(command).DoAsync(x => DownloadAsync(x.Item1, command));

    private async Task DownloadAsync(IDownloadJob job, IDownloadCommand command)
    {
        var resourceRepository = await _resourceRepositoryFactory.BuildAsync(job);
        var pageRetriever = _pageRetrieverFactory.Build(job);
        var page = await pageRetriever.GetPageAsync(command.PageUrl);

        var info = new ResourceInfo(page, command.PageIndex, command.ResourceUrl, command.ResourceIndex);
        if (await IsNotDownloadedAsync(info, resourceRepository, job.DownloadAlways))
        {
            var downloadStreamProvider = _downloadStreamProviderFactory.Build(job);
            var stream = await downloadStreamProvider.GetStreamAsync(command.ResourceUrl);
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
