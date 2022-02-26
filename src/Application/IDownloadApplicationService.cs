using Scrap.Domain.Jobs;

namespace Scrap.Application;

public interface IDownloadApplicationService
{
    Task DownloadAsync(NewJobDto jobDto, Uri pageUrl, int pageIndex, Uri resourceUrl, int resourceIndex);
}