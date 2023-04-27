using CommandLine;

namespace Scrap.CommandLine.Commands;

internal interface IFullScanOption
{
    [Option('f', "fullscan", Required = false, HelpText = "Navigate through already visited pages")]
    bool FullScan { get; }
}
