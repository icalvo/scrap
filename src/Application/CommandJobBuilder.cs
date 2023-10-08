using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Application;

public abstract class CommandJobBuilder<TCommand, TJob> : ICommandJobBuilder<TCommand, TJob>
    where TCommand : INameOrRootUrlCommand
{
    private readonly ISiteFactory _siteFactory;
    protected readonly IJobBuilder JobBuilder;


    public CommandJobBuilder(ISiteFactory siteFactory, IJobBuilder jobBuilder)
    {
        _siteFactory = siteFactory;
        JobBuilder = jobBuilder;
    }

    protected abstract Maybe<TJob> _f(Site site, Maybe<Uri> argRootUrl, TCommand command);
    public Task<Result<(TJob, Site), Unit>> Build(TCommand command)
    {
        return _siteFactory.BuildFromSite(command.NameOrRootUrl, (site, root) => _f(site, root, command).ToResult(Unit.Default));
    }
}