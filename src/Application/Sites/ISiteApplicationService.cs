using SharpX;

namespace Scrap.Application.Sites;

public interface ISiteApplicationService
{
    Task<Maybe<SiteDto>> FindByNameAsync(string siteName);
    IAsyncEnumerable<SiteDto> GetAllAsync();
    IAsyncEnumerable<SiteDto> FindByRootUrlAsync(string rootUrl);
}
