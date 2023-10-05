namespace Scrap.Application.Scrap.One;

public interface IScrapOneApplicationService
{
    Task ScrapAsync(IScrapOneCommand oneCommand);
}
