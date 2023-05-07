namespace Scrap.Application.Resources;

public interface IResourcesApplicationService
{
    IAsyncEnumerable<string> GetResourcesAsync(IResourceCommand resourceOneCommand);
}
