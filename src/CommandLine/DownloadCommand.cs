using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.Application;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DownloadCommand : AsyncCommandBase<DownloadSettings>
{
    public DownloadCommand(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem)
        : base(configuration, oAuthCodeGetter, fileSystem)
    {
    }

    protected override async Task<int> CommandExecuteAsync(DownloadSettings settings)
    {
        var serviceResolver = BuildServiceProviderWithoutConsole();
        var newJob = await BuildJobDtoAsync(serviceResolver, settings.Name, settings.RootUrl, false, settings.DownloadAlways, true, false);
        if (newJob == null)
        {
            return 1;
        }

        var scrapAppService = serviceResolver.GetRequiredService<IDownloadApplicationService>();
        var inputLines = settings.ResourceUrls ?? ConsoleInput();
        foreach (var line in inputLines)
        {
            var split = line.Split(" ");
            var pageIndex = int.Parse(split[0]);
            var pageUrl = new Uri(split[1]);
            var resourceIndex = int.Parse(split[2]);
            var resourceUrl = new Uri(split[3]);
            await scrapAppService.DownloadAsync(newJob, pageUrl, pageIndex, resourceUrl, resourceIndex);
            Console.WriteLine($"Downloaded {resourceUrl}");
        }

        return 0;
    }
}
