using SharpX;

namespace Scrap.Domain.Sites;

public interface ISiteRepository
{
    Task<Maybe<Site>> GetByNameAsync(string siteName);
    IAsyncEnumerable<Site> FindByRootUrlAsync(string rootUrl);
    IAsyncEnumerable<Site> ListAsync();
    IAsyncEnumerable<Site> GetScrappableAsync();
}
