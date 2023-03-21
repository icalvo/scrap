using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.Application;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ResourcesCommand : AsyncCommandBase<ResourcesSettings>
{
    public ResourcesCommand(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem)
        : base(configuration, oAuthCodeGetter, fileSystem)
    {
    }

    protected override async Task<int> CommandExecuteAsync(ResourcesSettings settings)
    {
        var serviceResolver = BuildServiceProviderWithoutConsole();
        var newJob = await BuildJobDtoAsync(serviceResolver, settings.Name, settings.RootUrl, false, false, true, true);
        if (newJob == null)
        {
            return 1;
        }

        var scrapAppService = serviceResolver.GetRequiredService<IResourcesApplicationService>();
        var pageIndex = 0;
        var inputLines = settings.PageUrls ?? ConsoleInput();
        foreach (var line in inputLines)
        {
            var pageUrl = new Uri(line);
            await scrapAppService.GetResourcesAsync(newJob, pageUrl, pageIndex).ForEachAsync(
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
