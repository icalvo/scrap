using System.Diagnostics.CodeAnalysis;
using Scrap.Application;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class TraverseCommand : AsyncCommand<TraverseSettings>
{
    private readonly IJobDtoBuilder _jobDtoBuilder;
    private readonly ITraversalApplicationService _traversalApplicationService;

    public TraverseCommand(IJobDtoBuilder jobDtoBuilder, ITraversalApplicationService traversalApplicationService)
    {
        _jobDtoBuilder = jobDtoBuilder;
        _traversalApplicationService = traversalApplicationService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, TraverseSettings settings)
    {
        var newJob = await _jobDtoBuilder.BuildJobDtoAsync(
            settings.Name,
            settings.RootUrl,
            settings.FullScan,
            false,
            true,
            true);
        if (newJob == null)
        {
            return 0;
        }

        await _traversalApplicationService.TraverseAsync(newJob).ForEachAsync(x => Console.WriteLine(x));

        return 0;
    }
}
