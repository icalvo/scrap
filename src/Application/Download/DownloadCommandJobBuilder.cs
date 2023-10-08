using Scrap.Domain.Jobs;
using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Application.Download;

public class DownloadCommandJobBuilder : CommandJobBuilder<IDownloadCommand, IDownloadJob>
{
    public DownloadCommandJobBuilder(ISiteFactory siteFactory, IJobBuilder jobBuilder)
        : base(siteFactory, jobBuilder)
    {
    }

    protected override Maybe<IDownloadJob> _f(Site site, Maybe<Uri> argRootUrl, IDownloadCommand command) =>
        JobBuilder.BuildDownloadJob(site, argRootUrl, command.DownloadAlways, false);
}