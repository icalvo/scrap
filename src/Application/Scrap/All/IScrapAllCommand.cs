namespace Scrap.Application.Scrap.All;

public interface IScrapAllCommand
{
    bool FullScan { get; }
    bool DownloadAlways { get; }
    bool DisableMarkingVisited { get; }

    bool DisableResourceWrites { get; }
}
