using Microsoft.Extensions.Logging;
namespace Scrap.Resources
{
    public interface IResourceRepositoryConfiguration
    {
        void Validate(ILoggerFactory loggerFactory);
        string Type { get; }
    }
}
