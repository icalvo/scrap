using Moq;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;

namespace Scrap.Tests.Unit;

public static class JobDtoBuilder
{
    public const string ResourceXPath = "//img/@src";

    public static JobDto Build(ResourceType resourceType, string resourceRepositoryType = "FileSystemRepository")
    {
        var resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>(
            x => x.RepositoryType == resourceRepositoryType);

        return new JobDto(
            null,
            ResourceXPath,
            resourceRepoConfig,
            "https://example.com",
            null,
            null,
            null,
            null,
            resourceType,
            null,
            null);
    }
}
