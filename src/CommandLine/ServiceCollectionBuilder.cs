using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.Application;
using Scrap.Domain;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

public class ServiceCollectionBuilder : IServiceCollectionBuilder
{
    private readonly IConfiguration _configuration;
    private readonly IOAuthCodeGetter _oAuthCodeGetter;

    public ServiceCollectionBuilder(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter)
    {
        _configuration = configuration;
        _oAuthCodeGetter = oAuthCodeGetter;
    }

    public IServiceCollection Build()
    {
        Debug.Assert(_configuration != null, nameof(_configuration) + " != null");
        return new ServiceCollection().ConfigureDomainServices().ConfigureApplicationServices()
            .ConfigureInfrastructureServices(_configuration, _oAuthCodeGetter);
    }
}
