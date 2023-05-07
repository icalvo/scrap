namespace Scrap.Application.Scrap.All;

public interface IScrapAllApplicationService
{
    Task ScrapAllAsync(IScrapAllCommand command);
}
