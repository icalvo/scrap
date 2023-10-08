using Moq;
using Scrap.Application;
using Scrap.Application.Download;
using Scrap.Application.Resources;
using Scrap.Application.Scrap;
using Scrap.Application.Scrap.One;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using Scrap.Domain.Sites;
using SharpX;
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
        var job = JobBuilder.BuildScrap(ResourceType.DownloadLink);

        var jobServiceMock = new Mock<ICommandJobBuilder<ISingleScrapCommand, ISingleScrapJob>>();

        jobServiceMock.SetupCommandJobBuilder(job, "asdf");

        var singleScrapServiceMock = new Mock<ISingleScrapService>();
        var service = new SingleScrapApplicationService(
            singleScrapServiceMock.Object,
            jobServiceMock.Object);

        await service.ScrapAsync(Mock.Of<ISingleScrapCommand>());

        singleScrapServiceMock.Verify(x => x.ExecuteJobAsync("asdf", job));
        singleScrapServiceMock.VerifyNoOtherCalls();
    }
}
