using System.Diagnostics.CodeAnalysis;
using Scrap.Application.Resources;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ResourcesVerb : IVerb<ResourcesVerb, ResourcesOptions>
{
    private readonly IResourcesApplicationService _resourcesApplicationService;

    public ResourcesVerb(IResourcesApplicationService resourcesApplicationService)
    {
        _resourcesApplicationService = resourcesApplicationService;
    }

    public async Task ExecuteAsync(ResourcesOptions options)
    {
        var pageIndex = 0;
        var inputLines = options.PageUrls.Length == 0 ? ConsoleTools.ConsoleInput() : options.PageUrls;
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            var cmd = new ResourceCommand(false, false, true, true, options.NameOrRootUrl, pageUrl, pageIndex);
            await _resourcesApplicationService.GetResourcesAsync(cmd).ForEachAsync(
                (resourceUrl, resourceIndex) =>
                {
                    var format = options.OnlyResourceLink ? "{3}" : "{0} {1} {2} {3}";
                    Console.WriteLine(format, pageIndex, pageUrl, resourceIndex, resourceUrl);
                });
            pageIndex++;
        }
    }
}
