using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrap.Application;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class AllCommand : ScrapCommandBase<AllSettings>
{
    public AllCommand(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem)
        : base(configuration, oAuthCodeGetter, fileSystem)
    {
    }

    protected override async Task<int> CommandExecuteAsync(AllSettings settings)
    {
        PrintHeader();

        var serviceResolver = BuildServiceProviderWithConsole();
        var logger = serviceResolver.GetRequiredService<ILogger<AllCommand>>();
        var definitionsApplicationService = serviceResolver.GetRequiredService<JobDefinitionsApplicationService>();

        var jobDefs = await definitionsApplicationService.GetAllAsync()
            .Where(x => x.RootUrl != null && x.HasResourceCapabilities()).ToListAsync();

        await ScrapMultipleJobDefsAsync(
            settings.FullScan,
            settings.DownloadAlways,
            settings.DisableMarkingVisited,
            settings.DisableResourceWrites,
            logger,
            true,
            jobDefs,
            null,
            null,
            serviceResolver);
        return 0;
    }
}
