using Microsoft.Extensions.Logging;
using Scrap.Application;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;

namespace Scrap.CommandLine.Commands;

internal static class ScrapCommandTools
{
    public static async Task ScrapMultipleJobDefsAsync(
        bool fullScan,
        bool downloadAlways,
        bool disableMarkingVisited,
        bool disableResourceWrites,
        ILogger logger,
        bool showJobDefs,
        IEnumerable<JobDefinitionDto> jobDefs,
        string? rootUrl,
        string? envRootUrl,
        IScrapApplicationService scrapApplicationService)
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
            logger.LogInformation("Starting {Definition}...", jobDef.Name);
            await scrapApplicationService.ScrapAsync(newJob);
            logger.LogInformation("Finished!");
        }
    }
}
