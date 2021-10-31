using System;
using System.Linq;
using LiteDB;

namespace Scrap.JobDefinitions
{
    public class LiteDbJobDefinition : JobDefinition
    {
        [BsonCtor]
        public LiteDbJobDefinition(string id, string adjacencyXPath, string adjacencyAttribute, string resourceXPath, string resourceAttribute, string resourceRepoType, BsonArray resourceRepoArgs, string? rootUrl, int httpRequestRetries, TimeSpan httpRequestDelayBetweenRetries)
            : base(adjacencyXPath, adjacencyAttribute, resourceXPath, resourceAttribute, resourceRepoType, resourceRepoArgs.Select(value => value.AsString).ToArray(), rootUrl, httpRequestRetries, httpRequestDelayBetweenRetries)
        {
            Id = id;
        }

        public LiteDbJobDefinition(string id, JobDefinition jobDefinition)
            : base(jobDefinition)
        {
            Id = id;
        }

        public string Id { get; }
    }
}