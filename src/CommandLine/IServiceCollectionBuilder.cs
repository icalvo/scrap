using Microsoft.Extensions.DependencyInjection;

namespace Scrap.CommandLine;

public interface IServiceCollectionBuilder
{
    IServiceCollection Build();
}
