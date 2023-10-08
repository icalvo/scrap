using Scrap.Common;
using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Domain;

public static class SiteJobBuilder
{
    public static Task<Result<(T, Site), Unit>> BuildFromSite<T>(
        this ISiteFactory siteFactory,
        Maybe<NameOrRootUrl> argNameOrRootUrl,
        Func<Site, Maybe<Uri>, Result<T, Unit>> buildJob)
    {
        return siteFactory.Build(argNameOrRootUrl)
                .BindAsync(
                site =>
                    buildJob(
                        site,
                        argNameOrRootUrl.Bind(nr => nr.MatchRootUrl(x => x)))
                    .Map(job => (job, site)));
    }
}
