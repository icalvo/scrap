namespace Scrap.JobDefinitions
{
    public class LiteDbJobDefinition : ScrapJobDefinition
    {
        public LiteDbJobDefinition(string id, string adjacencyXPath, string adjacencyAttribute, string resourceXPath, string resourceAttribute, string destinationRootFolder, string destinationExpression, string? rootUrl)
            : base(adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute, destinationRootFolder, destinationExpression, rootUrl)
        {
            Id = id;
        }

        public LiteDbJobDefinition(string id, ScrapJobDefinition scrapJobDefinition)
            : base(scrapJobDefinition)
        {
            Id = id;
        }

        public string Id { get; }
    }
}