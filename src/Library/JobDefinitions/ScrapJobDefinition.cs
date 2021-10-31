namespace Scrap.JobDefinitions
{
    public class ScrapJobDefinition
    {
        public ScrapJobDefinition(string adjacencyXPath, string adjacencyAttribute, string resourceXPath, string resourceAttribute, string destinationRootFolder, string destinationExpression, string? rootUrl)
        {
            AdjacencyXPath = adjacencyXPath;
            AdjacencyAttribute = adjacencyAttribute;
            ResourceXPath = resourceXPath;
            ResourceAttribute = resourceAttribute;
            DestinationRootFolder = destinationRootFolder;
            DestinationExpression = destinationExpression;
            RootUrl = rootUrl;
        }

        public ScrapJobDefinition(ScrapJobDefinition scrapJobDefinition)
            : this(scrapJobDefinition.AdjacencyXPath, scrapJobDefinition.AdjacencyAttribute, scrapJobDefinition.ResourceXPath, scrapJobDefinition.ResourceAttribute, scrapJobDefinition.DestinationRootFolder, scrapJobDefinition.DestinationExpression, scrapJobDefinition.RootUrl)
        {
        }

        public ScrapJobDefinition(ScrapJobDefinition scrapJobDefinition, string rootUrl)
            : this(scrapJobDefinition.AdjacencyXPath, scrapJobDefinition.AdjacencyAttribute, scrapJobDefinition.ResourceXPath, scrapJobDefinition.ResourceAttribute, scrapJobDefinition.DestinationRootFolder, scrapJobDefinition.DestinationExpression, scrapJobDefinition.RootUrl)
        {
            RootUrl = rootUrl;
        }

        public string? RootUrl { get; }
        public string AdjacencyXPath { get; }
        public string AdjacencyAttribute { get; }
        public string ResourceXPath { get; }
        public string ResourceAttribute { get; }
        public string DestinationRootFolder { get; }
        public string DestinationExpression { get; }
    }
}