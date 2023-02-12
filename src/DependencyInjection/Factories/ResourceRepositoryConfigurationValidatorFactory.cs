using Microsoft.Extensions.Logging;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.DependencyInjection.Factories;

public class ResourceRepositoryConfigurationValidatorFactory
    : IFactory<IResourceRepositoryConfiguration, IResourceRepositoryConfigurationValidator>
{
    private readonly ILoggerFactory _loggerFactory;

    public ResourceRepositoryConfigurationValidatorFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public IResourceRepositoryConfigurationValidator Build(IResourceRepositoryConfiguration resourceRepoArgs) =>
        resourceRepoArgs switch
        {
            FileSystemResourceRepositoryConfiguration config => new FileSystemResourceRepositoryConfigurationValidator(
                Singleton<CompiledDestinationProvider>.Get(
                    () => new CompiledDestinationProvider(
                        config,
                        _loggerFactory.CreateLogger<CompiledDestinationProvider>()))),
            _ => throw new InvalidOperationException(
                $"Unknown resource processor config type: {resourceRepoArgs.GetType().Name}")
        };
}
