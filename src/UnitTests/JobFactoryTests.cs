using FluentAssertions;
using Moq;
using Scrap.Domain;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Xunit;

namespace Scrap.Tests;

public class JobFactoryTests
{
    [Fact]
    public async Task CreateAsync()
    {
        var jobFactory =
            new JobFactory(Mock.Of<IEntityRegistry<Job>>(), new []
            {
                Mock.Of<IResourceRepositoryConfigurationValidator>(x => x.RepositoryType == "repoType")
            });
        IResourceRepositoryConfiguration resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>(x =>
            x.RepositoryType == "repoType");
       
        var actual = await jobFactory.CreateAsync(new JobDto(
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
    
    [Fact]
    public async Task CreateAsync_NoMatchingValidator_Throws()
    {
        var jobFactory =
            new JobFactory(Mock.Of<IEntityRegistry<Job>>(), new []
            {
                Mock.Of<IResourceRepositoryConfigurationValidator>(x => x.RepositoryType == "repoType")
            });
        IResourceRepositoryConfiguration resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>(x =>
            x.RepositoryType == "repoTypeNotMatching");
       
        var action = () => jobFactory.CreateAsync(new JobDto(
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

        await action.Should().ThrowAsync<Exception>();
    }    
}
