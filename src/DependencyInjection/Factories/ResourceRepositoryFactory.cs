using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.DependencyInjection.Factories;

public class ResourceRepositoryFactory : IFactory<Job, IResourceRepository>
{
    private readonly string? _baseRootFolder;
    private readonly ILoggerFactory _loggerFactory;

    public ResourceRepositoryFactory(string? baseRootFolder, ILoggerFactory loggerFactory)
    {
        _baseRootFolder = baseRootFolder;
        _loggerFactory = loggerFactory;
    }

    public IResourceRepository Build(Job job) =>
        job.ResourceRepoArgs switch
        {
            FileSystemResourceRepositoryConfiguration cfg => (IResourceRepository)new FileSystemResourceRepository(
                Singleton<CompiledDestinationProvider>.Get(
                    () => new CompiledDestinationProvider(
                        cfg,
                        _loggerFactory.CreateLogger<CompiledDestinationProvider>())),
                cfg,
                _loggerFactory.CreateLogger<FileSystemResourceRepository>(),
                job.DisableResourceWrites,
                _baseRootFolder),
            null => throw new ArgumentException("No Resource Repository found", nameof(job)),
            _ => throw new ArgumentException(
                $"Unknown resource processor config type: {job.ResourceRepoArgs.GetType().Name}",
                nameof(job))
        };
}
