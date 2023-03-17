using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.JobDefinitions.JsonFile;
using Scrap.Domain.Jobs;
using Scrap.Domain.Jobs.Graphs;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Domain;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureDomainServices(this IServiceCollection container)
    {
        container.AddSingleton<IGraphSearch, DepthFirstGraphSearch>();
        container.AddSingleton<IJobFactory, JobFactory>();
        
        container.AddSingleton<IJobDefinitionRepository>(
            sp =>
            {
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("InfrastructureServiceConfiguration");
                var config = sp.GetRequiredService<IConfiguration>();
                var definitionsFilePath = config[ConfigKeys.Definitions];
                if (definitionsFilePath == null)
                {
                    throw new Exception("No definitions file in the configuration!");
                }

                logger.LogDebug("Definitions file: {DefinitionsPath}", definitionsFilePath);
                return new MemoryJobDefinitionRepository(
                    definitionsFilePath,
                    sp.GetRequiredService<IFileSystemFactory>(),
                    sp.GetRequiredService<ILogger<MemoryJobDefinitionRepository>>());
            }); 
        return container;
    }
}
