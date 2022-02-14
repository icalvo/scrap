using Moq;
using Scrap.Pages;

namespace Scrap.Tests;

public static class TestTools
{

    public static IPage PageMock(string uri, XPath linksXPath, params string[] resourceLinks)
    {
        var mock = new Mock<IPage>(MockBehavior.Strict);
        mock.Setup(x => x.Uri)
            .Returns(new Uri(uri));
        mock.Setup(x => x.Links(linksXPath))
            .Returns(resourceLinks.Select(x => new Uri(x)));
        return mock.Object;
    }    
}