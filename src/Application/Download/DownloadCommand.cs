using Scrap.Domain;
using SharpX;

namespace Scrap.Application.Download;

public class DownloadCommand : IDownloadCommand
{
    public DownloadCommand(
        Maybe<NameOrRootUrl> nameOrRootUrl,
        bool downloadAlways,
        Uri pageUrl,
        int pageIndex,
        Uri resourceUrl,
        int resourceIndex)
    {
        NameOrRootUrl = nameOrRootUrl;
        DownloadAlways = downloadAlways;
        PageUrl = pageUrl;
        PageIndex = pageIndex;
        ResourceUrl = resourceUrl;
        ResourceIndex = resourceIndex;
    }

    public Maybe<NameOrRootUrl> NameOrRootUrl { get; }
    public bool DownloadAlways { get; }
    public Uri PageUrl { get; }
    public int PageIndex { get; }
    public Uri ResourceUrl { get; }
    public int ResourceIndex { get; }
}
