using Microsoft.Extensions.DependencyInjection;
using Scrap.Application.Scrap;

namespace Scrap.Application;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureApplicationServices(this IServiceCollection container)
    {
        container.AddTransient<IJobDefinitionsApplicationService, JobDefinitionsApplicationService>();
        container.AddTransient<IScrapApplicationService, ScrapApplicationService>();
        container.AddTransient<IScrapDownloadsService, ScrapDownloadsService>();
        container.AddTransient<IScrapTextService, ScrapTextService>();
        container.AddTransient<IDownloadApplicationService, DownloadApplicationService>();
        container.AddTransient<ITraversalApplicationService, TraversalApplicationService>();
        container.AddTransient<IResourcesApplicationService, ResourcesApplicationService>();
        container.AddSingleton<IVisitedPagesApplicationService, VisitedPagesApplicationService>();

        return container;
    }
}
