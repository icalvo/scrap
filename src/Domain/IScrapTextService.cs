using Scrap.Domain.Jobs;

namespace Scrap.Domain;

public interface IScrapTextService
{
    Task ScrapTextAsync(ISingleScrapJob job);
}
