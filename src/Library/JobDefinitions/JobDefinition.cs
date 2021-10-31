namespace Scrap.JobDefinitions
{
    public class JobDefinition
    {
        public JobDefinition(string adjacencyXPath, string adjacencyAttribute, string resourceXPath, string resourceAttribute, string resourceRepoType, string[] resourceRepoArgs, string? rootUrl)
        {
            AdjacencyXPath = adjacencyXPath;
            AdjacencyAttribute = adjacencyAttribute;
            ResourceXPath = resourceXPath;
            ResourceAttribute = resourceAttribute;
            ResourceRepoType = resourceRepoType;
            ResourceRepoArgs = resourceRepoArgs;
            RootUrl = rootUrl;
        }

        public JobDefinition(JobDefinition jobDefinition)
            : this(jobDefinition.AdjacencyXPath, jobDefinition.AdjacencyAttribute, jobDefinition.ResourceXPath, jobDefinition.ResourceAttribute, jobDefinition.ResourceRepoType, jobDefinition.ResourceRepoArgs, jobDefinition.RootUrl)
        {
        }

        public JobDefinition(JobDefinition jobDefinition, string rootUrl)
            : this(jobDefinition.AdjacencyXPath, jobDefinition.AdjacencyAttribute, jobDefinition.ResourceXPath, jobDefinition.ResourceAttribute, jobDefinition.ResourceRepoType, jobDefinition.ResourceRepoArgs, jobDefinition.RootUrl)
        {
            RootUrl = rootUrl;
        }

        public string? RootUrl { get; }
        public string AdjacencyXPath { get; }
        public string AdjacencyAttribute { get; }
        public string ResourceXPath { get; }
        public string ResourceAttribute { get; }
        public string ResourceRepoType { get; }
        public string[] ResourceRepoArgs { get; }
    }
}