using Scrap.Common;
using Scrap.Domain.Resources;

namespace Scrap.Domain.Jobs;

public interface IResourceRepositoryOptions
{
    AsyncLazy<IResourceRepositoryConfiguration> ResourceRepoArgs { get; }
    bool DisableResourceWrites { get; }
}