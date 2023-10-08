using FluentAssertions;
using Moq;
using Scrap.Application.Resources;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Sites;
using SharpX;
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
                JobBuilder.ResourceXPath,
                "https://example.com/1.txt",
                "https://example.com/2.txt"),
            new PageMock("https://example.com/b").ResourceLinks(
                JobBuilder.ResourceXPath,
                "https://example.com/3.txt",
                "https://example.com/4.txt"));
        var job = JobBuilder.BuildResources();
        builder.CommandJobBuilderMock.SetupCommandJobBuilder(job, "x");

        var service = builder.Build();
        ResourceCommand cmd = new(
            false,
            false,
            false,
            false,
            new NameOrRootUrl("").ToJust(),
            new Uri("https://example.com/a"),
            7);
        var actual = await service.GetResourcesAsync(cmd).ToArrayAsync();

        actual.Should().BeEquivalentTo("https://example.com/1.txt", "https://example.com/2.txt");
    }
}
