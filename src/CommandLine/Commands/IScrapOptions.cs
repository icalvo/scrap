using CommandLine;

namespace Scrap.CommandLine.Commands;

internal interface IScrapOptions : IDownloadAlwaysOption, IFullScanOption
{
    [Option('m', new[] { "disableMarkingVisited", "dmv" }, Required = false, HelpText = "Disable mark as visited")]
    bool DisableMarkingVisited { get; }

    [Option(
        'r',
        new[] { "disableResourceWrite", "drw" },
        Required = false,
        HelpText = "Do not actually download resources")]
    bool DisableResourceWrites { get; }
}
