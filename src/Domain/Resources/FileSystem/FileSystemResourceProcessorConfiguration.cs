using Microsoft.Extensions.Logging;
using Scrap.Downloads;
using Scrap.ResourceDownloaders;

namespace Scrap.Resources.FileSystem
{
    public class FileSystemResourceProcessorConfiguration : IResourceProcessorConfiguration
    {
        private FileSystemResourceProcessorConfiguration()
        {
        }

        public FileSystemResourceProcessorConfiguration(string[] destinationExpression, string destinationRootFolder)
        {
            DestinationExpression = destinationExpression;
            DestinationRootFolder = destinationRootFolder;
        }

        public string[] DestinationExpression { get; private set; } = null!;
        public string DestinationRootFolder { get; private set; } = null!;
        public string Type => "filesystem";

        public void Validate(ILoggerFactory loggerFactory)
        {
            _ = CompiledDestinationProvider.CreateCompiled(
                DestinationExpression,
                new Logger<CompiledDestinationProvider>(loggerFactory));
        }
    }
}