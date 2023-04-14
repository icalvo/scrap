using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;

namespace Scrap.CommandLine;

internal interface IJobDtoBuilder
{
    Task<JobDto?> BuildJobDtoAsync(
        string? name,
        string? rootUrl,
        bool? fullScan,
        bool? downloadAlways,
        bool? disableMarkingVisited,
        bool? disableResourceWrites);

    Task<JobDefinitionDto?> GetJobDefinitionAsync(string? name, string? rootUrl);
}
