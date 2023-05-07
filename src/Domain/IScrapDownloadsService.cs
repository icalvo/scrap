using Scrap.Domain.Jobs;

namespace Scrap.Domain;

public interface IScrapDownloadsService
{
    Task DownloadLinksAsync(Job job);
}
