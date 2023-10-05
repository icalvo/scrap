using Microsoft.Extensions.Logging;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Infrastructure.Factories;

public class ResourceRepositoryFactory : IResourceRepositoryFactory
{
    private readonly string? _baseRootFolder;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IFileSystemFactory _fileSystemFactory;
    private readonly IDestinationProviderFactory _destinationProviderFactory;
    private readonly ILogger<ResourceRepositoryFactory> _logger;

    public ResourceRepositoryFactory(
        string? baseRootFolder,
        ILoggerFactory loggerFactory,
        IFileSystemFactory fileSystemFactory,
        IDestinationProviderFactory destinationProviderFactory,
        ILogger<ResourceRepositoryFactory> logger)
    {
        _baseRootFolder = baseRootFolder;
        _loggerFactory = loggerFactory;
        _fileSystemFactory = fileSystemFactory;
        _destinationProviderFactory = destinationProviderFactory;
        _logger = logger;
    }

    public async Task<IResourceRepository> BuildAsync(Job job)
    {
        _logger.LogInformation("Resource File System: {FileSystemType}", _fileSystemFactory.FileSystemType);
        return await job.ResourceRepoArgs.ValueAsync() switch
        {
            FileSystemResourceRepositoryConfiguration cfg => (IResourceRepository)await FileSystemResourceRepository.BuildAsync(
                await _destinationProviderFactory.BuildAsync(cfg),
                cfg,
                _loggerFactory.CreateLogger<FileSystemResourceRepository>(),
                _baseRootFolder,
                _fileSystemFactory,
                job.DisableResourceWrites),
            null => throw new ArgumentException("No Resource Repository found", nameof(job)),
            _ => throw new ArgumentException(
                $"Unknown resource processor config type: {job.ResourceRepoArgs.GetType().Name}",
                nameof(job))
        };
    }
}
