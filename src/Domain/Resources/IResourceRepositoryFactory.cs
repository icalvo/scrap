using Scrap.Domain.Jobs;

namespace Scrap.Domain.Resources;

public interface IResourceRepositoryFactory
{
    public Task<IResourceRepository> BuildAsync(IResourceRepositoryOptions options);
}
