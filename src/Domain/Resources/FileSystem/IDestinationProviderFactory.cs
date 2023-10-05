namespace Scrap.Domain.Resources.FileSystem;

public interface IDestinationProviderFactory
{
    public Task<IDestinationProvider> BuildAsync(FileSystemResourceRepositoryConfiguration param);
}
