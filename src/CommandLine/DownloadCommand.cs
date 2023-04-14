using System.Diagnostics.CodeAnalysis;
using Scrap.Application;
using Spectre.Console.Cli;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DownloadCommand : AsyncCommand<DownloadSettings>
{
    private readonly IJobDtoBuilder _jobDtoBuilder;
    private readonly IDownloadApplicationService _downloadApplicationService;

    public DownloadCommand(IJobDtoBuilder jobDtoBuilder, IDownloadApplicationService downloadApplicationService)
    {
        _jobDtoBuilder = jobDtoBuilder;
        _downloadApplicationService = downloadApplicationService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DownloadSettings settings)
    {
        var newJob = await _jobDtoBuilder.BuildJobDtoAsync(
            settings.Name,
            settings.RootUrl,
            false,
            settings.DownloadAlways,
            true,
            false);
        if (newJob == null)
        {
            return 1;
        }

        var inputLines = settings.ResourceUrls ?? ConsoleTools.ConsoleInput();
        foreach (var line in inputLines)
        {
            var split = line.Split(" ");
            var pageIndex = int.Parse(split[0]);
            var pageUrl = new Uri(split[1]);
            var resourceIndex = int.Parse(split[2]);
            var resourceUrl = new Uri(split[3]);
            await _downloadApplicationService.DownloadAsync(newJob, pageUrl, pageIndex, resourceUrl, resourceIndex);
            Console.WriteLine($"Downloaded {resourceUrl}");
        }

        return 0;
    }
}
