namespace Scrap.Application.Download;

public interface IDownloadApplicationService
{
    Task DownloadAsync(IDownloadCommand command);
}
