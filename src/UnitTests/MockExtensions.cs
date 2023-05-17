using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using Scrap.Domain.Downloads;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit;

public static class MockExtensions
{
    public static void SetupWithString(this IDownloadStreamProvider s) =>
        s.GetStreamAsync(It.IsAny<Uri>()).Returns(new MemoryStream("some text goes here!.."u8.ToArray()));

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
