namespace Scrap.CommandLine;

internal interface IScrapSettings
{
    bool FullScan { get; }
    bool DownloadAlways { get; }
    bool DisableMarkingVisited { get; }
    bool DisableResourceWrites { get; }
}