namespace Scrap.Domain.Resources;

public interface IResourceRepositoryConfigurationValidator
{
    string RepositoryType { get; }
    Task ValidateAsync(IResourceRepositoryConfiguration config);
}
