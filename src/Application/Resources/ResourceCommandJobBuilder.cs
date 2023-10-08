using Scrap.Application.Download;
using Scrap.Domain.Jobs;
using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Application.Resources;

public class ResourceCommandJobBuilder : CommandJobBuilder<IResourceCommand, IResourcesJob>
{
    public ResourceCommandJobBuilder(ISiteFactory siteFactory, IJobBuilder jobBuilder)
        : base(siteFactory, jobBuilder)
    {
    }

    protected override Maybe<IResourcesJob> _f(Site site, Maybe<Uri> argRootUrl, IResourceCommand command) =>
        JobBuilder.BuildResourcesJob(site, argRootUrl);
}
