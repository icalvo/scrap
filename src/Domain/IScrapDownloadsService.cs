using Scrap.Domain.Jobs;

namespace Scrap.Domain;

public interface IScrapDownloadsService
{
    Task DownloadLinksAsync(ISingleScrapJob job);
}
