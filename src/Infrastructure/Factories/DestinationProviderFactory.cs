using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Infrastructure.Factories;

public class DestinationProviderFactory : IDestinationProviderFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IFileSystemFactory _fileSystemFactory;

    public DestinationProviderFactory(ILoggerFactory loggerFactory, IFileSystemFactory fileSystemFactory)
    {
        _loggerFactory = loggerFactory;
        _fileSystemFactory = fileSystemFactory;
    }

    public async Task<IDestinationProvider> BuildAsync(
        FileSystemResourceRepositoryConfiguration cfg)
    {
        return new CompiledDestinationProvider(cfg, await _fileSystemFactory.BuildAsync(true), _loggerFactory.CreateLogger<CompiledDestinationProvider>());
    }
}
