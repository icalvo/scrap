namespace Scrap.Domain.Pages;

public interface IPageRetriever
{
    Task<IPage> GetPageAsync(Uri uri, bool noCache);
    Task<IPage> GetPageAsync(Uri uri) => GetPageAsync(uri, false);
}
