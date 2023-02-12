using FluentAssertions;
using Moq;
using Scrap.Domain;
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
        var jobFactory = new JobFactory(
            Mock.Of<IFactory<IResourceRepositoryConfiguration, IResourceRepositoryConfigurationValidator>>(
                f => f.Build(It.IsAny<IResourceRepositoryConfiguration>()) ==
                     Mock.Of<IResourceRepositoryConfigurationValidator>()));
        var resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>();

        var actual = await jobFactory.Build(
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
        var jobFactory = new JobFactory(
            Mock.Of<IFactory<IResourceRepositoryConfiguration, IResourceRepositoryConfigurationValidator>>(
                f => f.Build(It.IsAny<IResourceRepositoryConfiguration>()) == failingValidator));

        var resourceRepoConfig = Mock.Of<IResourceRepositoryConfiguration>();

        var action = () => jobFactory.Build(
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
