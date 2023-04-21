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

    public async Task<int> ExecuteAsync(ResourcesOptions options)
    {
        var newJob = await _jobDtoBuilder.BuildJobDtoAsync(options.Name, options.RootUrl, false, false, true, true);
        if (newJob == null)
        {
            return 1;
        }

        var pageIndex = 0;
        var inputLines = options.PageUrls ?? ConsoleTools.ConsoleInput();
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

        return 0;
    }
}
