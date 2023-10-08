using Scrap.Application.Download;
using Scrap.Domain.Jobs;
using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Application.Scrap.One;

public class SingleScrapCommandJobBuilder : CommandJobBuilder<ISingleScrapCommand, ISingleScrapJob>
{
    public SingleScrapCommandJobBuilder(ISiteFactory siteFactory, IJobBuilder jobBuilder)
        : base(siteFactory, jobBuilder)
    {
    }

    protected override Maybe<ISingleScrapJob> _f(Site site, Maybe<Uri> argRootUrl, ISingleScrapCommand command) =>
        JobBuilder.BuildSingleScrapJob(site, argRootUrl, command.FullScan, command.DownloadAlways, command.DisableMarkingVisited, command.DisableResourceWrites);
}
