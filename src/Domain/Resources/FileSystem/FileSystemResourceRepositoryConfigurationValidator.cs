namespace Scrap.Domain.Resources.FileSystem;

public class FileSystemResourceRepositoryConfigurationValidator
    : BaseResourceRepositoryConfigurationValidator<FileSystemResourceRepository>
{
    private readonly IDestinationProvider _destinationProvider;

    public FileSystemResourceRepositoryConfigurationValidator(IDestinationProvider destinationProvider)
    {
        _destinationProvider = destinationProvider;
    }

    public override Task ValidateAsync(IResourceRepositoryConfiguration config) =>
        _destinationProvider.ValidateAsync((FileSystemResourceRepositoryConfiguration)config);
}
