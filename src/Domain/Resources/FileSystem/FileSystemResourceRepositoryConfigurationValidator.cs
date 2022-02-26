namespace Scrap.Domain.Resources.FileSystem;

public class FileSystemResourceRepositoryConfigurationValidator : IResourceRepositoryConfigurationValidator
{
    private readonly IDestinationProvider _destinationProvider;

    public FileSystemResourceRepositoryConfigurationValidator(IDestinationProvider destinationProvider)
    {
        _destinationProvider = destinationProvider;
    }

    public Task ValidateAsync(IResourceRepositoryConfiguration config)
    {
        return _destinationProvider.CompileAsync((FileSystemResourceRepositoryConfiguration)config);
    }
}