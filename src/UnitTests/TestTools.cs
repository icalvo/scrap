using Moq;
using Scrap.Domain;
using Scrap.Domain.Pages;

namespace Scrap.Tests;

public static class TestTools
{
    public static IPage PageMock(
        string uri,
        XPath linkXPath,
        params string[] linkResults)
    {
        var mock = new Mock<IPage>(MockBehavior.Strict);
        mock.Setup(x => x.Uri)
            .Returns(new Uri(uri));

        mock.Setup(x => x.Links(linkXPath))
            .Returns(linkResults.Select(x => new Uri(x)));
        return mock.Object;
    }
    
    public static IPage PageMock(
        string uri,
        XPath linkXPath,
        string[] linkResults,
        XPath resourceXPath,
        string[] resourceResults,
        XPath contentsXPath,
        string[] contentsResults)
    {
        var mock = new Mock<IPage>(MockBehavior.Strict);
        mock.Setup(x => x.Uri)
            .Returns(new Uri(uri));

        mock.Setup(x => x.Links(linkXPath))
            .Returns(linkResults.Select(x => new Uri(x)));
        mock.Setup(x => x.Links(resourceXPath))
            .Returns(resourceResults.Select(x => new Uri(x)));
        mock.Setup(x => x.Contents(contentsXPath))
            .Returns(contentsResults);
        return mock.Object;
    }    
}
