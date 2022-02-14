namespace Scrap.Pages;

public interface IPageRetriever
{
    Task<IPage> GetPageAsync(Uri uri);
}
