namespace Scrap.Application.Scrap.One;

public interface ISingleScrapApplicationService
{
    Task ScrapAsync(ISingleScrapCommand command);
}
