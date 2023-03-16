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
        var vf = new Mock<IResourceRepositoryConfigurationValidatorFactory>();
        vf.Setup(f => f.BuildAsync(It.IsAny<IResourceRepositoryConfiguration>()))
            .ReturnsAsync(Mock.Of<IResourceRepositoryConfigurationValidator>());
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
        var failingValidator = new Mock<IResourceRepositoryConfigurationValidator>();
        failingValidator.Setup(x => x.ValidateAsync(It.IsAny<IResourceRepositoryConfiguration>()))
            .ThrowsAsync(new Exception());
        var vf = new Mock<IResourceRepositoryConfigurationValidatorFactory>();
        vf.Setup(f => f.BuildAsync(It.IsAny<IResourceRepositoryConfiguration>()))
            .ReturnsAsync(failingValidator.Object);
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
