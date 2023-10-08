using Microsoft.Extensions.DependencyInjection;
using Scrap.Application.Download;
using Scrap.Application.Resources;
using Scrap.Application.Scrap;
using Scrap.Application.Scrap.All;
using Scrap.Application.Scrap.One;
using Scrap.Application.Sites;
using Scrap.Application.Traversal;
using Scrap.Application.VisitedPages;
using Scrap.Domain.Jobs;

namespace Scrap.Application;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureApplicationServices(this IServiceCollection container)
    {
        container.AddTransient<ISiteApplicationService, SiteApplicationService>();
        container.AddTransient<ISingleScrapApplicationService, SingleScrapApplicationService>();
        container.AddTransient<IScrapAllApplicationService, ScrapAllApplicationService>();
        container.AddTransient<IDownloadApplicationService, DownloadApplicationService>();
        container.AddTransient<ITraversalApplicationService, TraversalApplicationService>();
        container.AddTransient<IResourcesApplicationService, ResourcesApplicationService>();
        container.AddSingleton<IVisitedPagesApplicationService, VisitedPagesApplicationService>();
        container.AddSingleton<ISingleScrapService, SingleScrapService>();
        container.AddSingleton<ICommandJobBuilder<IDownloadCommand, IDownloadJob>, DownloadCommandJobBuilder>();
        container.AddSingleton<ICommandJobBuilder<IResourceCommand, IResourcesJob>, ResourceCommandJobBuilder>();
        container.AddSingleton<ICommandJobBuilder<ITraverseCommand, ITraverseJob>, TraverseCommandJobBuilder>();
        container.AddSingleton<ICommandJobBuilder<ISingleScrapCommand, ISingleScrapJob>, SingleScrapCommandJobBuilder>();
        return container;
    }
}
