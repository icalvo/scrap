namespace Scrap.Domain.Resources;

public interface IResourceRepositoryConfigurationValidator
{
    Task ValidateAsync(IResourceRepositoryConfiguration config);
    string RepositoryType { get; }
}
