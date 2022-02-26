using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.ApplicationServices;

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
        var builder = new MockBuilder(
            _output,
            new PageMock("https://example.com/a")
                .ResourceLinks(MockBuilder.ResourceXPath, "https://example.com/1.txt", "https://example.com/2.txt"),
            new PageMock("https://example.com/b")
                .ResourceLinks(MockBuilder.ResourceXPath, "https://example.com/3.txt", "https://example.com/4.txt"));
        var jobDto = builder.BuildJobDto();
        var service = builder.BuildResourcesApplicationService(jobDto);

        var actual = await service.GetResourcesAsync(jobDto, new Uri("https://example.com/a"), 7).ToArrayAsync();

        actual.Should().BeEquivalentTo("https://example.com/1.txt", "https://example.com/2.txt");
    }
}
