using System.Diagnostics.CodeAnalysis;
using Scrap.Application;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ResourcesCommand : AsyncCommand<ResourcesSettings>
{
    private readonly IJobDtoBuilder _jobDtoBuilder;
    private readonly IResourcesApplicationService _resourcesApplicationService;

    public ResourcesCommand(IJobDtoBuilder jobDtoBuilder, IResourcesApplicationService resourcesApplicationService)
    {
        _jobDtoBuilder = jobDtoBuilder;
        _resourcesApplicationService = resourcesApplicationService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ResourcesSettings settings)
    {
        var newJob = await _jobDtoBuilder.BuildJobDtoAsync(settings.Name, settings.RootUrl, false, false, true, true);
        if (newJob == null)
        {
            return 1;
        }

        var pageIndex = 0;
        var inputLines = settings.PageUrls ?? ConsoleTools.ConsoleInput();
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            await _resourcesApplicationService.GetResourcesAsync(newJob, pageUrl, pageIndex).ForEachAsync(
                (resourceUrl, resourceIndex) =>
                {
                    var format = settings.OnlyResourceLink ? "{3}" : "{0} {1} {2} {3}";
                    Console.WriteLine(format, pageIndex, pageUrl, resourceIndex, resourceUrl);
                });
            pageIndex++;
        }

        return 0;
    }
}
