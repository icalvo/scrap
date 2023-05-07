using Scrap.Domain;
using SharpX;

namespace Scrap.Application.Download;

public interface IDownloadCommand
{
    Maybe<NameOrRootUrl> NameOrRootUrl { get; }
    bool DownloadAlways { get; }
    Uri PageUrl { get; }
    int PageIndex { get; }
    Uri ResourceUrl { get; }
    int ResourceIndex { get; }
}
