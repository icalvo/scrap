namespace Scrap.Domain.Resources;

public interface IResourceRepositoryConfigurationValidator
{
    Task ValidateAsync(IResourceRepositoryConfiguration configuration);
}
