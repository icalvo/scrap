using Moq;
using Scrap.Application.Scrap;
using Scrap.Application.Scrap.One;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit.ApplicationServices;

public class ScrapOneApplicationServiceTests
{
    private readonly ITestOutputHelper _output;

    public ScrapOneApplicationServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ScrapAsync()
    {
        var job = JobBuilder.Build(ResourceType.DownloadLink);

        var jobServiceMock = new Mock<IJobBuilder>();
        jobServiceMock.SetupWithJob(job, "asdf");
        var singleScrapServiceMock = new Mock<ISingleScrapService>();
        var service = new ScrapOneApplicationService(jobServiceMock.Object, singleScrapServiceMock.Object);

        await service.ScrapAsync(Mock.Of<IScrapOneCommand>());

        singleScrapServiceMock.Verify(x => x.ExecuteJobAsync("asdf", job));
        singleScrapServiceMock.VerifyNoOtherCalls();
    }
}
