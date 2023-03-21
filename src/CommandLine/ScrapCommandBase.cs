using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrap.Application.Scrap;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

internal abstract class ScrapCommandBase<TSettings> : AsyncCommandBase<TSettings> where TSettings : SettingsBase, IScrapSettings
{
    protected ScrapCommandBase(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem)
        : base(configuration, oAuthCodeGetter, fileSystem)
    {
    }

    protected static async Task ScrapMultipleJobDefsAsync(
        bool fullScan,
        bool downloadAlways,
        bool disableMarkingVisited,
        bool disableResourceWrites,
        ILogger logger,
        bool showJobDefs,
        IEnumerable<JobDefinitionDto> jobDefs,
        string? rootUrl,
        string? envRootUrl,
        IServiceProvider serviceResolver)
    {
        var jobDefsArray = jobDefs as JobDefinitionDto[] ?? jobDefs.ToArray();
        if (!jobDefsArray.Any())
        {
            logger.LogWarning("No job definition found, nothing will be done");
            return;
        }

        if (showJobDefs)
        {
            logger.LogInformation(
                "The following job def(s). will be run: {JobDefs}",
                string.Join(", ", jobDefsArray.Select(x => x.Name)));
        }

        foreach (var jobDef in jobDefsArray)
        {
            var newJob = new JobDto(
                jobDef,
                rootUrl ?? envRootUrl,
                fullScan,
                null,
                downloadAlways,
                disableMarkingVisited,
                disableResourceWrites);
            var scrapAppService = serviceResolver.GetRequiredService<ScrapApplicationService>();
            logger.LogInformation("Starting {Definition}...", jobDef.Name);
            await scrapAppService.ScrapAsync(newJob);
            logger.LogInformation("Finished!");
        }
    }
}
