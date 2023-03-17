using FluentAssertions;
using Moq;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Xunit;

namespace Scrap.Tests.Unit;

public class JobFactoryTests
{
    [Fact]
    public async Task Build()
    {
        var vf = new Mock<IResourceRepositoryConfigurationValidator>();
        var jobFactory = new JobFactory(vf.Object);
        var resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>();

        var actual = await jobFactory.BuildAsync(
            new JobDto(
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
    public void Build_ValidationFails_Throws()
    {
        var vf = new Mock<IResourceRepositoryConfigurationValidator>();
        vf.Setup(f => f.ValidateAsync(It.IsAny<IResourceRepositoryConfiguration>()))
            .ThrowsAsync(new Exception());
        var jobFactory = new JobFactory(vf.Object);

        var resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>();

        var action = () => jobFactory.BuildAsync(
            new JobDto(
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

        action.Should().ThrowAsync<Exception>();
    }
}
