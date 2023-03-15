using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.DependencyInjection.Factories;

public class ResourceRepositoryConfigurationValidatorFactory
    : IResourceRepositoryConfigurationValidatorFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IFileSystemFactory _fileSystemFactory;

    public ResourceRepositoryConfigurationValidatorFactory(ILoggerFactory loggerFactory, IFileSystemFactory fileSystemFactory)
    {
        _loggerFactory = loggerFactory;
        _fileSystemFactory = fileSystemFactory;
    }

    public async Task<IResourceRepositoryConfigurationValidator> BuildAsync(IResourceRepositoryConfiguration resourceRepoArgs)
    {
        var _fileSystem = await _fileSystemFactory.BuildAsync();
        return resourceRepoArgs switch
        {
            FileSystemResourceRepositoryConfiguration config => new FileSystemResourceRepositoryConfigurationValidator(
                Singleton<CompiledDestinationProvider>.Get(
                    () => new CompiledDestinationProvider(
                        config,
                        _fileSystem,
                        _loggerFactory.CreateLogger<CompiledDestinationProvider>()))),
            _ => throw new InvalidOperationException(
                $"Unknown resource processor config type: {resourceRepoArgs.GetType().Name}")
        };
    }
}
