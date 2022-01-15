namespace Scrap.Pages;

public interface IPageRetriever
{
    Task<Page> GetPageAsync(Uri uri);
}