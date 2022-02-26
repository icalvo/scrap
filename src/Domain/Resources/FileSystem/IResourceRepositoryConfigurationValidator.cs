namespace Scrap.Domain.Resources.FileSystem;

public interface IResourceRepositoryConfigurationValidator
{
    Task ValidateAsync(IResourceRepositoryConfiguration config);
}