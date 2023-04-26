using System.Diagnostics.CodeAnalysis;
using Scrap.Application;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ResourcesCommand : ICommand<ResourcesCommand, ResourcesOptions>
{
    private readonly IJobDtoBuilder _jobDtoBuilder;
    private readonly IResourcesApplicationService _resourcesApplicationService;

    public ResourcesCommand(IJobDtoBuilder jobDtoBuilder, IResourcesApplicationService resourcesApplicationService)
    {
        _jobDtoBuilder = jobDtoBuilder;
        _resourcesApplicationService = resourcesApplicationService;
    }

    public async Task ExecuteAsync(ResourcesOptions options)
    {
        var newJob = await _jobDtoBuilder.BuildJobDtoAsync(options.Name, options.RootUrl, false, false, true, true);
        if (newJob == null)
        {
            throw new CommandException(1, "Couldn't build a job definition with the provided arguments");
        }

        var pageIndex = 0;
        var inputLines = options.PageUrls.Length == 0 ? ConsoleTools.ConsoleInput() : options.PageUrls;
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            await _resourcesApplicationService.GetResourcesAsync(newJob, pageUrl, pageIndex).ForEachAsync(
                (resourceUrl, resourceIndex) =>
                {
                    var format = options.OnlyResourceLink ? "{3}" : "{0} {1} {2} {3}";
                    Console.WriteLine(format, pageIndex, pageUrl, resourceIndex, resourceUrl);
                });
            pageIndex++;
        }
    }
}
