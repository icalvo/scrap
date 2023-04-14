using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scrap.Application;
using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ScrapCommand : AsyncCommand<ScrapSettings>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ScrapCommand> _logger;
    private readonly IJobDtoBuilder _jobDtoBuilder;
    private readonly IScrapApplicationService _scrapApplicationService;

    public ScrapCommand(
        IConfiguration configuration,
        ILogger<ScrapCommand> logger,
        IJobDtoBuilder jobDtoBuilder,
        IScrapApplicationService scrapApplicationService)
    {
        _configuration = configuration;
        _logger = logger;
        _jobDtoBuilder = jobDtoBuilder;
        _scrapApplicationService = scrapApplicationService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ScrapSettings settings)
    {
        ConsoleTools.PrintHeader();

        var jobDef = await _jobDtoBuilder.GetJobDefinitionAsync(settings.Name, settings.RootUrl);

        var jobDefs = jobDef == null ? Array.Empty<JobDefinitionDto>() : new[] { jobDef };

        await ScrapCommandTools.ScrapMultipleJobDefsAsync(
            settings.FullScan,
            settings.DownloadAlways,
            settings.DisableMarkingVisited,
            settings.DisableResourceWrites,
            _logger,
            settings.Name != null,
            jobDefs,
            settings.RootUrl,
            _configuration.JobDefRootUrl(),
            _scrapApplicationService);

        return 0;
    }
}
