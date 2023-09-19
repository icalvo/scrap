using Microsoft.Extensions.Logging;
using NSubstitute;
using Scrap.Application.Scrap;
using Scrap.Application.Scrap.All;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Resources;
using Scrap.Domain.Sites;
using Xunit;

namespace Scrap.Tests.Unit.ApplicationServices;

public class ScrapAllApplicationServiceTests
{
    [Fact]
    public async Task ScrapAllAsync()
    {
        var job = JobBuilder.Build(ResourceType.DownloadLink);

        var jobService = Substitute.For<IJobBuilder>();
        var siteRepository = Substitute.For<ISiteRepository>();
        siteRepository.GetScrappableAsync().Returns(
            new[]
            {
                new Site(
                    "C",
                    rootUrl: new Uri("https://C.com"),
                    resourceXPath: "xp",
                    resourceRepoArgs: Substitute.For<IResourceRepositoryConfiguration?>()),
                new Site(
                    "D",
                    rootUrl: new Uri("https://D.com"),
                    resourceXPath: "xp",
                    resourceRepoArgs: Substitute.For<IResourceRepositoryConfiguration?>())
            }.ToAsyncEnumerable());

        jobService.BuildJobAsync(
            Arg.Is<Site>(y => y.Name == "C" || y.Name == "D"),
                null,
                null,
                false,
                false,
                false,
            false).Returns(job);
        var singleScrapService = Substitute.For<ISingleScrapService>();

        var service = new ScrapAllApplicationService(
            jobService,
            Substitute.For<ILogger<ScrapAllApplicationService>>(),
            singleScrapService,
            siteRepository);

        await service.ScrapAllAsync(Substitute.For<IScrapAllCommand>());

        await singleScrapService.Received(2).ExecuteJobAsync(Arg.Any<string>(), Arg.Any<Job>());
        await singleScrapService.Received().ExecuteJobAsync("C", job);
        await singleScrapService.Received().ExecuteJobAsync("D", job);
    }
}
