using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;
using Scrap.JobDefinitions.JsonFile;
using Scrap.Jobs.Graphs;

namespace Scrap.DependencyInjection;

public class ServicesResolver
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _config;
    private readonly ILogger _logger;

    public ServicesResolver(ILoggerFactory loggerFactory, IConfiguration config)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<ServicesResolver>();
        _config = config;
        _logger.LogDebug("Scrap DB: {ConnectionString}", _config["Scrap:Database"]);
    }

    public async Task<JobDefinitionsApplicationService> GetJobDefinitionsApplicationServiceAsync()
    {
        _logger.LogDebug("Definitions file: {DefinitionsPath}", _config["Scrap:Definitions"]);
        return
            new JobDefinitionsApplicationService(
                await MemoryJobDefinitionRepository.FromJsonFileAsync(_config["Scrap:Definitions"]),
                _loggerFactory.CreateLogger<JobDefinitionsApplicationService>(),
                _loggerFactory);
    }

    public JobApplicationService GetJobApplicationService()
    {
        return Singleton.Get(() => new JobApplicationService(
            new DepthFirstGraphSearch(),
            new JobServicesFactory(_loggerFactory, _config),
            new JobFactory(_loggerFactory),
            _loggerFactory.CreateLogger<JobApplicationService>()));
    }
}
