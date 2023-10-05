using Scrap.Domain.Jobs;

namespace Scrap.Application.Scrap;

public interface ISingleScrapService
{
    Task ExecuteJobAsync(string siteName, ISingleScrapJob job);
}
