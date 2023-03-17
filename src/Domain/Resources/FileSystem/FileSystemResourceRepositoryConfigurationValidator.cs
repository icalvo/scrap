namespace Scrap.Domain.Resources.FileSystem;

public class FileSystemResourceRepositoryConfigurationValidator
{
    private readonly IDestinationProvider _destinationProvider;

    public FileSystemResourceRepositoryConfigurationValidator(IDestinationProvider destinationProvider)
    {
        _destinationProvider = destinationProvider;
    }

    public Task ValidateAsync() => _destinationProvider.ValidateAsync();
}
