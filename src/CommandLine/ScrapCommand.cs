using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrap.Application;
using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ScrapCommand : ScrapCommandBase<ScrapSettings>
{
    public ScrapCommand(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem)
        : base(configuration, oAuthCodeGetter, fileSystem)
    {
    }

    protected override async Task<int> CommandExecuteAsync([NotNull] ScrapSettings settings)
    {
        PrintHeader();

        var serviceResolver = BuildServiceProviderWithConsole();
        var logger = serviceResolver.GetRequiredService<ILogger<ScrapCommand>>();
        var definitionsApplicationService = serviceResolver.GetRequiredService<JobDefinitionsApplicationService>();

        var envRootUrl = Configuration[ConfigKeys.JobDefRootUrl];
        var envName = Configuration[ConfigKeys.JobDefName];

        var jobDef = await GetJobDefinitionAsync(
            settings.Name,
            settings.RootUrl,
            definitionsApplicationService,
            envName,
            envRootUrl,
            logger);

        var jobDefs = jobDef == null ? Array.Empty<JobDefinitionDto>() : new[] { jobDef };

        await ScrapMultipleJobDefsAsync(
            settings.FullScan,
            settings.DownloadAlways,
            settings.DisableMarkingVisited,
            settings.DisableResourceWrites,
            logger,
            settings.Name != null,
            jobDefs,
            settings.RootUrl,
            envRootUrl,
            serviceResolver);

        return 0;
    }
}
