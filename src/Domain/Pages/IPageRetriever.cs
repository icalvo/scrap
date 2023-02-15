namespace Scrap.Domain.Pages;

public interface IPageRetriever
{
    Task<IPage> GetPageAsync(Uri uri);
    Task<IPage> GetPageWithoutCacheAsync(Uri uri);
}
