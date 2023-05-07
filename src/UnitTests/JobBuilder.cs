using Moq;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;

namespace Scrap.Tests.Unit;

public static class JobBuilder
{
    public const string ResourceXPath = "//img/@src";

    public static Job Build(ResourceType resourceType, string resourceRepositoryType = "FileSystemRepository")
    {
        var resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>(
            x => x.RepositoryType == resourceRepositoryType);

        return new Job(
            new Uri("https://example.com"),
            resourceType,
            resourceXPath: ResourceXPath,
            resourceRepository: resourceRepoConfig);
    }
}
