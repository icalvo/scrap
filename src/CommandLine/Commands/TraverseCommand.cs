using System.Diagnostics.CodeAnalysis;
using Scrap.Application;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class TraverseCommand : ICommand<TraverseCommand, TraverseOptions>
{
    private readonly IJobDtoBuilder _jobDtoBuilder;
    private readonly ITraversalApplicationService _traversalApplicationService;

    public TraverseCommand(IJobDtoBuilder jobDtoBuilder, ITraversalApplicationService traversalApplicationService)
    {
        _jobDtoBuilder = jobDtoBuilder;
        _traversalApplicationService = traversalApplicationService;
    }

    public async Task ExecuteAsync(TraverseOptions options)
    {
        var newJob = await _jobDtoBuilder.BuildJobDtoAsync(
            options.Name,
            options.RootUrl,
            options.FullScan,
            false,
            true,
            true);
        if (newJob == null)
        {
            throw new CommandException(1, "Couldn't build a job definition with the provided arguments");
        }

        await _traversalApplicationService.TraverseAsync(newJob).ForEachAsync(x => Console.WriteLine(x));
    }
}
