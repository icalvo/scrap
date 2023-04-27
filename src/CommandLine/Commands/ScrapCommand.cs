﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scrap.Application;
using Scrap.Domain;
using Scrap.Domain.JobDefinitions;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ScrapCommand : ICommand<ScrapCommand, ScrapOptions>
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

    public async Task ExecuteAsync(ScrapOptions settings)
    {
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
    }
}