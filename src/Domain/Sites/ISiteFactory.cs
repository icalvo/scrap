using SharpX;

namespace Scrap.Domain.Sites;

public interface ISiteFactory
{
    Task<Result<Site, Unit>> Build(
        Maybe<NameOrRootUrl> argNameOrRootUrl);
}
