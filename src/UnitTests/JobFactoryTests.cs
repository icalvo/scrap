using FluentAssertions;
using Moq;
using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Scrap.Domain.Resources.FileSystem;
using Xunit;

namespace Scrap.Tests;

public class JobFactoryTests
{
    [Fact]
    public async Task CreateAsync()
    {
        var jobFactory =
            new JobFactory(Mock.Of<IEntityRegistry<Job>>(), Mock.Of<IResourceRepositoryConfigurationValidator>());
        IResourceRepositoryConfiguration resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>();
       
        var actual = await jobFactory.CreateAsync(new NewJobDto(
            null,
            "//a/@href",
            resourceRepoConfig,
            "https://example.com",
            null,
            null,
            null,
            null,
            ResourceType.DownloadLink,
            null,
            null));

        actual.HttpRequestRetries.Should().Be(5);
    }
}
