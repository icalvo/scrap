using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Scrap.Application;
using Scrap.Application.Download;
using Scrap.Domain;
using Scrap.Domain.Downloads;
using Scrap.Domain.Jobs;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;
using SharpX;
using Xunit;
using Xunit.Abstractions;

namespace Scrap.Tests.Integration;

public class DownloadApplicationServiceTests
{
    private readonly ITestOutputHelper _output;

    public DownloadApplicationServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DownloadAsync_HappyPath()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new KeyValuePair<string, string?>[]
            {
                new("Logging:LogLevel:Default", "Trace"),
                new("Console:FormatterName", "CustomConsoleFormatter"),
                new("Console:FormatterOptions:IncludeScopes", "true"),
                new("Console:FormatterOptions:EnableColors", "false"),
                new("Scrap:Sites", "./sites.json"),
                new("Scrap:Database", "./scrap.db"),
                new("Scrap:FileSystemType", "local"),
                new("Scrap:BaseRootFolder", "./test-results"),
            }).Build();

        var pageMock = new Mock<IPage>();
        var visitedPageRepoMock = new Mock<IVisitedPageRepository>();
        var fileSystemMock = new Mock<IRawFileSystem>();
        var pageRetrieverMock = new Mock<IPageRetriever>();
        var downloadStreamProviderMock = new Mock<IDownloadStreamProvider>();
        var mocksToShowInvocations = new Mock[]
        {
            pageMock, pageRetrieverMock, fileSystemMock, visitedPageRepoMock, downloadStreamProviderMock
        };

        // Setup factories
        var visitedPageRepoFactoryMock = new Mock<IVisitedPageRepositoryFactory>();
        visitedPageRepoFactoryMock.Setup(x => x.Build()).Returns(visitedPageRepoMock.Object);
        visitedPageRepoFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(visitedPageRepoMock.Object);
        visitedPageRepoFactoryMock.Setup(x => x.Build(It.IsAny<DatabaseInfo>())).Returns(visitedPageRepoMock.Object);
        var fileSystemFactoryMock = new Mock<IFileSystemFactory>();
        fileSystemFactoryMock.Setup(x => x.BuildAsync(It.IsAny<bool?>()))
            .ReturnsAsync(new FileSystem(fileSystemMock.Object));
        var pageRetrieverFactoryMock = new Mock<IPageRetrieverFactory>();
        pageRetrieverFactoryMock.Setup(x => x.Build(It.IsAny<Job>())).Returns(pageRetrieverMock.Object);
        var downloadStreamProviderFactoryMock = new Mock<IDownloadStreamProviderFactory>();
        downloadStreamProviderFactoryMock.Setup(x => x.Build(It.IsAny<Job>()))
            .Returns(downloadStreamProviderMock.Object);

        var sc = new ServiceCollection();

        var sl = sc.ConfigureDomainServices().ConfigureApplicationServices().ConfigureInfrastructureServices(
            config,
            Mock.Of<IOAuthCodeGetter>(),
            visitedPageRepoFactoryMock.Object,
            fileSystemFactoryMock.Object,
            pageRetrieverFactoryMock.Object,
            downloadStreamProviderFactoryMock.Object).AddLogging().BuildServiceProvider();

        fileSystemMock.Setup(x => x.PathNormalizeFolderSeparator("Docs\\Example")).Returns("Docs/Example");
        fileSystemMock.Setup(x => x.PathCombine(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>(
            (baseDirectory, filePath) => $"{baseDirectory}/{filePath}");

        fileSystemMock.Setup(x => x.PathReplaceForbiddenChars(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((path, _) => path);
        fileSystemMock
            .Setup(
                x => x.PathGetRelativePath(
                    "./test-results/Docs/Example",
                    "./test-results/Docs/Example/firlollo/22.jpg")).Returns("/firlollo/22.jpg");
        fileSystemMock.Setup(x => x.FileExistsAsync("./test-results/Docs/Example/firlollo/22.jpg")).ReturnsAsync(false);
        fileSystemMock.Setup(x => x.PathGetDirectoryName("./test-results/Docs/Example/firlollo/22.jpg"))
            .Returns("./test-results/Docs/Example/firlollo");
        fileSystemMock.Setup(x => x.IsReadOnly).Returns(false);
        fileSystemMock.Setup(x => x.DirectoryCreateAsync("./test-results/Docs/Example/firlollo"))
            .Returns(Task.CompletedTask);
        fileSystemMock.Setup(
            x => x.FileWriteAsync("./test-results/Docs/Example/firlollo/22.jpg", It.IsAny<MemoryStream>()))
        .Returns(Task.CompletedTask);
        pageMock.Setup(x => x.Content("//*[contains(@class, 'post-title ')]/text()")).Returns("firlollo");
        pageRetrieverMock.Setup(x => x.GetPageAsync(new Uri("https://example.com/PageUrl")))
            .ReturnsAsync(pageMock.Object);
        downloadStreamProviderMock.Setup(x => x.GetStreamAsync(new Uri("https://example.com/ResUrl")))
            .ReturnsAsync(new MemoryStream("download"u8.ToArray()));

        const string definitions = """
[
  {
    "name": "Example",
    "rootUrl": "https://example.com",
    "adjacencyXPath": "N/A",
    "resourceXPath": "(//*[contains(@class, 'post-body')]//a[contains(@href,'.jpg') or contains(@href,'.gif') or contains(@href,'/img/')])/@href",
    "resourceRepository": {
      "type": "filesystem",
      "rootFolder": "Docs\\Example",
        "pathFragments": [
            "page.Content(\"//*[contains(@class, 'post-title ')]/text()\").Trim()",
            "resourceIndex + (String.IsNullOrEmpty(resourceUrl.Extension()) ? \".jpg\" : resourceUrl.Extension())"
        ]
    }
  }
]
""";
        fileSystemMock.Setup(x => x.FileOpenReadAsync("./sites.json"))
            .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes(definitions)));

        var svc = sl.GetRequiredService<IDownloadApplicationService>();
        try
        {
            await svc.DownloadAsync(
                new DownloadCommand(
                    new NameOrRootUrl("Example").ToJust(),
                    false,
                    new Uri("https://example.com/PageUrl"),
                    12,
                    new Uri("https://example.com/ResUrl"),
                    22));
        }
        finally
        {
            var invocations = mocksToShowInvocations.SelectMany(x => x.Invocations);
            _output.WriteLine("MOCK INVOCATIONS");
            foreach (var invocation in invocations)
            {
                object? rawReturnValue = invocation.ReturnValue;
                bool isTask = false;
                if (invocation.ReturnValue is Task t)
                {
                    isTask = true;
                    rawReturnValue = t.Result();
                }

                var returnValue = rawReturnValue is null ? "null" : rawReturnValue.ToString();

                if (isTask)
                {
                    returnValue = $"TASK -> {returnValue}";
                }

                if (invocation.MatchingSetup is null)
                {
                    _output.WriteLine("{0} -> NOT_SETUP ({1})", invocation, returnValue);
                }
                else
                {
                    _output.WriteLine("{0} -> {1}", invocation, returnValue);
                }
            }
        }
    }
}