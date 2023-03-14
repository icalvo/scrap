using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Common;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Xunit.Abstractions;

namespace Scrap.Tests.Unit;

public static class MockExtensions
{
    public static void SetupFactory<TIn, TOut>(this Mock<IAsyncFactory<TIn, TOut>> mock, TOut build) =>
        mock.Setup(x => x.Build(It.IsAny<TIn>())).ReturnsAsync(build);

    public static void SetupFactory<TIn, TOut>(this Mock<IFactory<TIn, TOut>> mock, TOut build) =>
        mock.Setup(x => x.Build(It.IsAny<TIn>())).Returns(build);

    public static void SetupFactory<TIn1, TIn2, TOut>(this Mock<IFactory<TIn1, TIn2, TOut>> mock, TOut build) =>
        mock.Setup(x => x.Build(It.IsAny<TIn1>(), It.IsAny<TIn2>())).Returns(build);

    public static void SetupFactory<TIn, TOut>(this Mock<IOptionalParameterFactory<TIn, TOut>> mock, TOut build)
        where TIn : class
    {
        mock.Setup(x => x.Build()).Returns(build);
        mock.Setup(x => x.Build(It.IsAny<TIn>())).Returns(build);
    }

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
