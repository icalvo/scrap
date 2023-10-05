using Scrap.Domain;
using SharpX;

namespace Scrap.Application.Resources;

public class ResourceCommand : IResourceCommand
{
    public ResourceCommand(
        bool downloadAlways,
        bool fullScan,
        bool disableMarkingVisited,
        bool disableResourceWrites,
        Maybe<NameOrRootUrl> nameOrRootUrl,
        Uri pageUrl,
        int pageIndex)
    {
        DownloadAlways = downloadAlways;
        FullScan = fullScan;
        DisableMarkingVisited = disableMarkingVisited;
        DisableResourceWrites = disableResourceWrites;
        NameOrRootUrl = nameOrRootUrl;
        PageUrl = pageUrl;
        PageIndex = pageIndex;
    }

    public bool DownloadAlways { get; }
    public bool FullScan { get; }
    public bool DisableMarkingVisited { get; }
    public bool DisableResourceWrites { get; }
    public Maybe<NameOrRootUrl> NameOrRootUrl { get; }
    public Uri PageUrl { get; }
    public int PageIndex { get; }
}
