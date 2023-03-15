using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.DependencyInjection.Factories;

public class ResourceRepositoryFactory : IResourceRepositoryFactory
{
    private readonly string? _baseRootFolder;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IFileSystemFactory _fileSystemFactory;
    public ResourceRepositoryFactory(
        string? baseRootFolder,
        ILoggerFactory loggerFactory,
        IFileSystemFactory fileSystemFactory)
    {
        _baseRootFolder = baseRootFolder;
        _loggerFactory = loggerFactory;
        _fileSystemFactory = fileSystemFactory;
    }

    public async Task<IResourceRepository> BuildAsync(Job job)
    {
        var fileSystem = await _fileSystemFactory.BuildAsync(job.DisableResourceWrites);
        return job.ResourceRepoArgs switch
        {
            FileSystemResourceRepositoryConfiguration cfg => (IResourceRepository)new FileSystemResourceRepository(
                Singleton<CompiledDestinationProvider>.Get(
                    () => new CompiledDestinationProvider(
                        cfg,
                        fileSystem,
                        _loggerFactory.CreateLogger<CompiledDestinationProvider>())),
                cfg,
                _loggerFactory.CreateLogger<FileSystemResourceRepository>(),
                _baseRootFolder,
                fileSystem),
            null => throw new ArgumentException("No Resource Repository found", nameof(job)),
            _ => throw new ArgumentException(
                $"Unknown resource processor config type: {job.ResourceRepoArgs.GetType().Name}",
                nameof(job))
        };
    }
}
