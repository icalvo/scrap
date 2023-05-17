using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Application.Scrap;
using Scrap.Application.Scrap.All;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Scrap.Domain.Sites;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class ScrapAllApplicationServiceTests
{
    private readonly ITestOutputHelper _output;

    public ScrapAllApplicationServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ScrapAllAsync()
    {
        var job = JobBuilder.Build(ResourceType.DownloadLink);

        var jobServiceMock = new Mock<IJobService>();
        var siteRepositoryMock = new Mock<ISiteRepository>();
        siteRepositoryMock.Setup(x => x.GetScrappableAsync()).Returns(
            new[]
            {
                new Site(
                    "C",
                    rootUrl: new Uri("http://C.com"),
                    resourceXPath: "xp",
                    resourceRepoArgs: Mock.Of<IResourceRepositoryConfiguration?>()),
                new Site(
                    "D",
                    rootUrl: new Uri("http://D.com"),
                    resourceXPath: "xp",
                    resourceRepoArgs: Mock.Of<IResourceRepositoryConfiguration?>())
            }.ToAsyncEnumerable());

        jobServiceMock.Setup(
            x => x.BuildJobAsync(
                It.Is<Site>(y => y.Name == "C" || y.Name == "D"),
                null,
                null,
                false,
                false,
                false,
                false)).ReturnsAsync(job);
        var singleScrapServiceMock = new Mock<ISingleScrapService>();

        var service = new ScrapAllApplicationService(
            jobServiceMock.Object,
            Mock.Of<ILogger<ScrapAllApplicationService>>(),
            singleScrapServiceMock.Object,
            siteRepositoryMock.Object);

        await service.ScrapAllAsync(Mock.Of<IScrapAllCommand>());

        singleScrapServiceMock.Verify(x => x.ExecuteJobAsync("C", job));
        singleScrapServiceMock.Verify(x => x.ExecuteJobAsync("D", job));
        singleScrapServiceMock.VerifyNoOtherCalls();
    }
}
