using Moq;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Tests.Unit;

public static class JobBuilder
{
    public static XPath ResourceXPath = "//img/@src";
    
    public static IResourcesJob BuildResources() =>
        Mock.Of<IResourcesJob>(j => j.ResourceXPath == ResourceXPath);

    public static ISingleScrapJob BuildScrap(ResourceType resourceType) =>
        Mock.Of<ISingleScrapJob>(j => j.ResourceXPath == ResourceXPath && j.ResourceType == resourceType);
}
