using Microsoft.Extensions.Logging;
namespace Scrap.Resources;

public interface IResourceRepositoryConfiguration
{
    Task ValidateAsync(ILoggerFactory loggerFactory);
    string Type { get; }
}
