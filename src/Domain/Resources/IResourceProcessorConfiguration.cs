using Microsoft.Extensions.Logging;
namespace Scrap.Resources
{
    public interface IResourceProcessorConfiguration
    {
        void Validate(ILoggerFactory loggerFactory);
        string Type { get; }
    }
}