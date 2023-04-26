using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Scrap.Application;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class AllCommand : ICommand<AllCommand, AllOptions>
{
    private readonly IGlobalConfigurationChecker _checker;
    private readonly ILogger<AllCommand> _logger;
    private readonly IJobDefinitionsApplicationService _jobDefinitionsApplicationService;
    private readonly IScrapApplicationService _scrapApplicationService;

    public AllCommand(
        IGlobalConfigurationChecker checker,
        ILogger<AllCommand> logger,
        IJobDefinitionsApplicationService jobDefinitionsApplicationService,
        IScrapApplicationService scrapApplicationService)
    {
        _checker = checker;
        _logger = logger;
        _jobDefinitionsApplicationService = jobDefinitionsApplicationService;
        _scrapApplicationService = scrapApplicationService;
    }

    public async Task ExecuteAsync(AllOptions options)
    {
        await _checker.EnsureGlobalConfigurationAsync();
        var jobDefs = await _jobDefinitionsApplicationService.GetAllAsync()
            .Where(x => x.RootUrl != null && x.HasResourceCapabilities()).ToListAsync();

        await ScrapCommandTools.ScrapMultipleJobDefsAsync(
            options.FullScan,
            options.DownloadAlways,
            options.DisableMarkingVisited,
            options.DisableResourceWrites,
            _logger,
            true,
            jobDefs,
            null,
            null,
            _scrapApplicationService);
    }
}
