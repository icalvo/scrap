namespace Scrap.Application.Download;

public interface IDownloadCommand : INameOrRootUrlCommand
{
    bool DownloadAlways { get; }
    Uri PageUrl { get; }
    int PageIndex { get; }
    Uri ResourceUrl { get; }
    int ResourceIndex { get; }
}
