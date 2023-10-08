using Scrap.Application.Download;
using Scrap.Domain.Jobs;
using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Application.Traversal;

public class TraverseCommandJobBuilder : CommandJobBuilder<ITraverseCommand, ITraverseJob>
{
    public TraverseCommandJobBuilder(ISiteFactory siteFactory, IJobBuilder jobBuilder)
        : base(siteFactory, jobBuilder)
    {
    }

    protected override Maybe<ITraverseJob> _f(Site site, Maybe<Uri> argRootUrl, ITraverseCommand command) =>
        JobBuilder.BuildTraverseJob(site, argRootUrl, disableMarkingVisited: false, command.FullScan);
}
