using Scrap.Pages;

namespace Scrap.Resources;

public class ResourceInfo
{
    public ResourceInfo(IPage page, int pageIndex, Uri resourceUrl, int resourceIndex)
    {
        this.Page = page;
        this.PageIndex = pageIndex;
        this.ResourceUrl = resourceUrl;
        this.ResourceIndex = resourceIndex;
    }
    public IPage Page { get; }
    public int PageIndex { get; }
    public Uri ResourceUrl { get; }
    public int ResourceIndex { get; }

    public void Deconstruct(out IPage page, out int pageIndex, out Uri resourceUrl, out int resourceIndex)
    {
        page = this.Page;
        pageIndex = this.PageIndex;
        resourceUrl = this.ResourceUrl;
        resourceIndex = this.ResourceIndex;
    }
}