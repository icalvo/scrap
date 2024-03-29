using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Domain.Resources;

public class ResourceRepositoryConfigurationValidator
    : IResourceRepositoryConfigurationValidator
{
    private readonly IDestinationProviderFactory _destinationProviderFactory;

    public ResourceRepositoryConfigurationValidator(IDestinationProviderFactory destinationProviderFactory)
    {
        _destinationProviderFactory = destinationProviderFactory;
    }

    public async Task ValidateAsync(IResourceRepositoryConfiguration? configuration)
    {
        switch (configuration)
        {
            case FileSystemResourceRepositoryConfiguration config:
                await (await _destinationProviderFactory.BuildAsync(config)).ValidateAsync();
                break;
            case null:
                throw new ArgumentNullException(nameof(configuration), "There is no resource repo config");
            default:
                throw new InvalidOperationException(
                    $"Unknown resource repo config type: {configuration.GetType().Name}");
        }
    }
}
