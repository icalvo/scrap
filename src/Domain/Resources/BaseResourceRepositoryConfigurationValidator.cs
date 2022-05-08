namespace Scrap.Domain.Resources;

public abstract class BaseResourceRepositoryConfigurationValidator<TRepository> : IResourceRepositoryConfigurationValidator
    where TRepository : IResourceRepository
{
    public abstract Task ValidateAsync(IResourceRepositoryConfiguration config);
    public string RepositoryType => typeof(TRepository).Name;
}
