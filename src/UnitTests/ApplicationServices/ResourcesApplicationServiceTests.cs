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

public static class MockExtensions
{
    public static void SetupWithJob(this Mock<ISiteService> mock, Job job, string siteName) =>
        mock.Setup(
            x => x.BuildJobAsync(
                It.IsAny<Maybe<NameOrRootUrl>>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>())).ReturnsAsync((job, siteName).ToJust());
}
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
        var job = JobBuilder.Build(ResourceType.DownloadLink);
        builder.SiteServiceMock.SetupWithJob(job, "x");
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
