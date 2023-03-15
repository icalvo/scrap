using FluentAssertions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Moq;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources.FileSystem;
using Xunit;

namespace Scrap.Tests.Unit;

public class CompiledDestinationProviderTests
{
    [Fact]
    public async Task GetDestinationAsync()
    {
        var fsMock = new Mock<IFileSystem>();
        fsMock.Setup(x => x.PathCombine(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((baseDirectory, filePath) => baseDirectory + ":" + filePath);
        var p = new CompiledDestinationProvider(
            new FileSystemResourceRepositoryConfiguration(
                new[]
                {
                    "resourceIndex + (String.IsNullOrEmpty(resourceUrl.Extension()) ? \".unknown\" : resourceUrl.Extension())"
                },
                "rootFolder"),
            fsMock.Object,
            Mock.Of<ILogger<CompiledDestinationProvider>>());
        var doc = new HtmlDocument();
        doc.LoadHtml(
            @"<html>
<body>
    <div id=""downloadCount"" data=""expectedData"">t1<div>t2a<span>t2b</span><span>t2c</span></div>t3</div>
    <a href=""a""></a>
    <a href=""b""><span>t4</span></a>
</body>
</html>");

        var page = new Page(new Uri("https://example.com"), doc, Mock.Of<IPageRetriever>(), Mock.Of<ILogger<Page>>());

        var result = await p.GetDestinationAsync(
            "destinationRootFolder",
            page,
            2,
            new Uri("https://example.com/resource.gif"),
            3);

        result.Should().Be("destinationRootFolder:3.gif");
    }
}
