using System.Diagnostics.CodeAnalysis;
using Scrap.Application;

namespace Scrap.CommandLine.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DownloadCommand : ICommand<DownloadCommand, DownloadOptions>
{
    private readonly IGlobalConfigurationChecker _checker;
    private readonly IJobDtoBuilder _jobDtoBuilder;
    private readonly IDownloadApplicationService _downloadApplicationService;

    public DownloadCommand(
        IGlobalConfigurationChecker checker,
        IJobDtoBuilder jobDtoBuilder,
        IDownloadApplicationService downloadApplicationService)
    {
        _checker = checker;
        _jobDtoBuilder = jobDtoBuilder;
        _downloadApplicationService = downloadApplicationService;
    }

    public async Task ExecuteAsync(DownloadOptions settings)
    {
        await _checker.EnsureGlobalConfigurationAsync();
        var newJob = await _jobDtoBuilder.BuildJobDtoAsync(
            settings.Name,
            settings.RootUrl,
            false,
            settings.DownloadAlways,
            true,
            false);
        if (newJob == null)
        {
            throw new CommandException(1, "Couldn't build a job definition with the provided arguments");
        }

        var inputLines = settings.ResourceLines ?? ConsoleTools.ConsoleInput();
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
    }
}
