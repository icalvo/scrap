namespace Scrap.Domain.Pages;

public class PageMarker
{
    public PageMarker(string uri)
    {
        Uri = uri;
    }

    public string Uri { get; }    
}
