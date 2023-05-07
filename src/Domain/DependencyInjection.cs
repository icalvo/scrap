using Microsoft.Extensions.DependencyInjection;
using Scrap.Common.Graphs;
using Scrap.Domain.Sites;

namespace Scrap.Domain;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureDomainServices(this IServiceCollection container)
    {
        container.AddSingleton<IGraphSearch, DepthFirstGraphSearch>();

        container.AddSingleton<ISiteService, SiteService>();
        container.AddTransient<IScrapDownloadsService, ScrapDownloadsService>();
        container.AddTransient<IScrapTextService, ScrapTextService>();
        
        return container;
    }
}
