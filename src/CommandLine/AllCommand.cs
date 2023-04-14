using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Scrap.Application;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class AllCommand : AsyncCommand<AllSettings>
{
    private readonly ILogger<AllCommand> _logger;
    private readonly JobDefinitionsApplicationService _jobDefinitionsApplicationService;
    private readonly IScrapApplicationService _scrapApplicationService;

    public AllCommand(
        ILogger<AllCommand> logger,
        JobDefinitionsApplicationService jobDefinitionsApplicationService,
        IScrapApplicationService scrapApplicationService)
    {
        _logger = logger;
        _jobDefinitionsApplicationService = jobDefinitionsApplicationService;
        _scrapApplicationService = scrapApplicationService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, AllSettings settings)
    {
        ConsoleTools.PrintHeader();

        var jobDefs = await _jobDefinitionsApplicationService.GetAllAsync()
            .Where(x => x.RootUrl != null && x.HasResourceCapabilities()).ToListAsync();

        await ScrapCommandTools.ScrapMultipleJobDefsAsync(
            settings.FullScan,
            settings.DownloadAlways,
            settings.DisableMarkingVisited,
            settings.DisableResourceWrites,
            _logger,
            true,
            jobDefs,
            null,
            null,
            _scrapApplicationService);

        return 0;
    }
}
