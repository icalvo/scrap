namespace Scrap.Domain.Resources;

public abstract class BaseResourceRepositoryConfiguration<TRepository> : IResourceRepositoryConfiguration
    where TRepository : IResourceRepository
{
    public string RepositoryType => typeof(TRepository).Name;
}