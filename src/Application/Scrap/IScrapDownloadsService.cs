using Scrap.Domain.Jobs;

namespace Scrap.Application.Scrap;

public interface IScrapDownloadsService
{
    Task DownloadLinksAsync(JobDto jobDto);
}
