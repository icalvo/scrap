using Scrap.Domain;
using Scrap.Domain.Resources;

namespace Scrap.DependencyInjection.Factories;

public class ResourceRepositoryConfigurationJobFactory : IFactory<IResourceRepoArgs, IResourceRepositoryConfiguration>
{
    public IResourceRepositoryConfiguration Build(IResourceRepoArgs job)
    {
        return job.ResourceRepoArgs;
    }
}