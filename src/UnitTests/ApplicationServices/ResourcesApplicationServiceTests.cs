using FluentAssertions;
using Moq;
using Scrap.Domain.JobDefinitions;
using Scrap.Domain.Jobs;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class ResourcesApplicationServiceTests
{
    private readonly ITestOutputHelper _output;

    public ResourcesApplicationServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task GetResourcesAsync()
    {
        var builder = new ResourcesApplicationServiceMockBuilder();
        builder.SetupTraversal(
            new PageMock("https://example.com/a").ResourceLinks(
                JobDtoBuilder.ResourceXPath,
                "https://example.com/1.txt",
                "https://example.com/2.txt"),
            new PageMock("https://example.com/b").ResourceLinks(
                JobDtoBuilder.ResourceXPath,
                "https://example.com/3.txt",
                "https://example.com/4.txt"));
        var jobDto = JobDtoBuilder.Build(ResourceType.DownloadLink);
        builder.JobFactoryMock.Setup(x => x.BuildAsync(It.IsAny<JobDto>())).ReturnsAsync(new Job(jobDto));
        var service = builder.Build();

        var actual = await service.GetResourcesAsync(jobDto, new Uri("https://example.com/a"), 7).ToArrayAsync();

        actual.Should().BeEquivalentTo("https://example.com/1.txt", "https://example.com/2.txt");
    }
}
