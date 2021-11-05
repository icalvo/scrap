namespace Scrap.Resources.FileSystem
{
    public class FileSystemResourceRepositoryConfiguration : IResourceRepositoryConfiguration
    {
        public FileSystemResourceRepositoryConfiguration(string destinationExpression, string destinationRootFolder)
        {
            DestinationExpression = destinationExpression;
            DestinationRootFolder = destinationRootFolder;
        }

        public string DestinationExpression { get; }
        public string DestinationRootFolder { get; }
        public string Type => "filesystem";
    }
}