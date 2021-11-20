using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Scrap.Resources.FileSystem
{
    public class FileSystemResourceRepositoryConfiguration : IResourceRepositoryConfiguration
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Deserialization by Hangfire")]
        private FileSystemResourceRepositoryConfiguration()
        {
        }

        public FileSystemResourceRepositoryConfiguration(string[] destinationExpression, string destinationRootFolder)
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
