namespace Scrap.Domain.Pages;

public interface IPageRetriever
{
    Task<IPage> GetPageAsync(Uri uri);
}
