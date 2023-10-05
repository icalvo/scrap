namespace Scrap.Domain.Pages;

public class VisitedPage
{
    public VisitedPage(string uri)
    {
        Uri = uri;
    }

    public string Uri { get; }
}
