using Scrap.Domain;
using SharpX;

namespace Scrap.Application.Scrap.One;

class SingleScrapCommand : ISingleScrapCommand
{
    public SingleScrapCommand(bool fullScan, bool downloadAlways, bool disableMarkingVisited, bool disableResourceWrites, Maybe<NameOrRootUrl> nameOrRootUrl)
    {
        FullScan = fullScan;
        DownloadAlways = downloadAlways;
        DisableMarkingVisited = disableMarkingVisited;
        DisableResourceWrites = disableResourceWrites;
        NameOrRootUrl = nameOrRootUrl;
    }
    public bool FullScan { get; }
    public bool DownloadAlways { get; }
    public bool DisableMarkingVisited { get; }
    public bool DisableResourceWrites { get; }
    public Maybe<NameOrRootUrl> NameOrRootUrl { get; }
}