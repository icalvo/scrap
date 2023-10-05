using Scrap.Common;

namespace Scrap.Domain.Resources.FileSystem;

public interface IDestinationProviderFactory : IAsyncFactory<FileSystemResourceRepositoryConfiguration, IDestinationProvider>
{
}
