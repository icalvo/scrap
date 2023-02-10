using Scrap.Domain.Pages;

namespace Scrap.Domain.Resources;

public class ResourceInfo
{
    public ResourceInfo(IPage page, int pageIndex, Uri resourceUrl, int resourceIndex)
    {
        Page = page;
        PageIndex = pageIndex;
        ResourceUrl = resourceUrl;
        ResourceIndex = resourceIndex;
    }

    public IPage Page { get; }
    public int PageIndex { get; }
    public Uri ResourceUrl { get; }
    public int ResourceIndex { get; }

    public void Deconstruct(out IPage page, out int pageIndex, out Uri resourceUrl, out int resourceIndex)
    {
        page = Page;
        pageIndex = PageIndex;
        resourceUrl = ResourceUrl;
        resourceIndex = ResourceIndex;
    }
}
