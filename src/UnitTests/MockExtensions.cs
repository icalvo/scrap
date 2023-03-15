using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Common;
using Scrap.Domain.Downloads;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit;

public static class MockExtensions
{
    public static void SetupWithString(this Mock<IDownloadStreamProvider> s) =>
        s.Setup(y => y.GetStreamAsync(It.IsAny<Uri>()))
            .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("some text goes here!..")));

    public static void SetupWithOutput(this Mock<ILogger> loggerMock, ITestOutputHelper output) =>
        loggerMock.Setup(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>())).Callback(
            (LogLevel _, EventId _, object state, Exception? _, object _) => output.WriteLine(state.ToString()));
}
