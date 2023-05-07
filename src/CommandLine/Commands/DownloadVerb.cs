using System.Diagnostics.CodeAnalysis;
using Scrap.Application.Download;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DownloadVerb : IVerb<DownloadVerb, DownloadOptions>
{
    private readonly IGlobalConfigurationChecker _checker;
    private readonly IDownloadApplicationService _downloadApplicationService;

    public DownloadVerb(
        IGlobalConfigurationChecker checker,
        IDownloadApplicationService downloadApplicationService)
    {
        _checker = checker;
        _downloadApplicationService = downloadApplicationService;
    }

    public async Task ExecuteAsync(DownloadOptions settings)
    {
        await _checker.EnsureGlobalConfigurationAsync();

        var inputLines = settings.ResourceLines ?? ConsoleTools.ConsoleInput();
        foreach (var line in inputLines)
        {
            var split = line.Split(" ");
            var pageIndex = int.Parse(split[0]);
            var pageUrl = new Uri(split[1]);
            var resourceIndex = int.Parse(split[2]);
            var resourceUrl = new Uri(split[3]);

            var cmd = new DownloadCommand(
                settings.NameOrRootUrl,
                settings.DownloadAlways,
                pageUrl,
                pageIndex,
                resourceUrl,
                resourceIndex);
            await _downloadApplicationService.DownloadAsync(cmd);
            Console.WriteLine($"Downloaded {resourceUrl}");
        }
    }
}
