using Scrap.Domain.Jobs;

namespace Scrap.Application;

public interface IScrapApplicationService
{
    Task ScrapAsync(JobDto jobDto);
}
