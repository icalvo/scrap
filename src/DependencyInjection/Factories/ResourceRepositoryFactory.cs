using Microsoft.Extensions.Logging;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.DependencyInjection.Factories;

public class ResourceRepositoryFactory : IFactory<Job, IResourceRepository>
{
    private readonly ILoggerFactory _loggerFactory;

    public ResourceRepositoryFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public IResourceRepository Build(Job job) =>
        job.ResourceRepoArgs switch
        {
            FileSystemResourceRepositoryConfiguration cfg =>
                (IResourceRepository)new FileSystemResourceRepository(
                    Singleton<CompiledDestinationProvider>.Get(() => 
                        new CompiledDestinationProvider(
                            cfg,
                            _loggerFactory.CreateLogger<CompiledDestinationProvider>())),
                    cfg,
                    _loggerFactory.CreateLogger<FileSystemResourceRepository>(),
                    job.DisableResourceWrites),
            null => throw new ArgumentException(
                $"Resource processor config not provided", nameof(job)), 
            _ => throw new ArgumentException(
                $"Unknown resource processor config type: {job.ResourceRepoArgs.GetType().Name}", nameof(job))
        };
}

public class Singleton<T>
{
    private static T? _item;
    private static readonly object Lock = new(); 
    
    public static T Get(Func<T> constructor)
    {
        lock (Lock)
        {
            _item ??= constructor();
        }

        return _item;
    }
}
