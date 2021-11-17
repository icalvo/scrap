using Microsoft.Extensions.Logging;
using Scrap.Downloads;
using Scrap.ResourceDownloaders;

namespace Scrap.Resources
{
    public class ListResourceProcessorConfiguration : IResourceProcessorConfiguration
    {
        private ListResourceProcessorConfiguration()
        {
        }

        public ListResourceProcessorConfiguration(string listPath)
        {
            ListPath = listPath;
        }

        public string ListPath { get; private set; } = null!;

        public void Validate(ILoggerFactory loggerFactory)
        {
        }

        public string Type => "list";
    }
}