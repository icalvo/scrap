using Moq;
using Scrap.Domain;
using Scrap.Domain.Jobs;
using SharpX;

namespace Scrap.Tests.Unit.ApplicationServices;

public static class MockExtensions
{
    public static void SetupWithJob(this Mock<IJobService> mock, Job job, string siteName) =>
        mock.Setup(
            x => x.BuildJobAsync(
                It.IsAny<Maybe<NameOrRootUrl>>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>())).ReturnsAsync((job, siteName).ToJust());
}
