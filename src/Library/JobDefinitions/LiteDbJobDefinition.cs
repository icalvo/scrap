using System;
using Scrap.Resources;

namespace Scrap.JobDefinitions
{
    public class LiteDbJobDefinition
    {
        public LiteDbJobDefinition() {}

        public LiteDbJobDefinition(string id, JobDefinition jobDefinition)
        {
            Id = id;
            AdjacencyXPath = jobDefinition.AdjacencyXPath;
            AdjacencyAttribute = jobDefinition.AdjacencyAttribute;
            ResourceXPath = jobDefinition.ResourceXPath;
            ResourceAttribute = jobDefinition.ResourceAttribute;
            ResourceRepoArgs = jobDefinition.ResourceRepoArgs;
            RootUrl = jobDefinition.RootUrl;
            HttpRequestRetries = jobDefinition.HttpRequestRetries;
            HttpRequestDelayBetweenRetries = jobDefinition.HttpRequestDelayBetweenRetries;
            WhatIf = jobDefinition.WhatIf;
            FullScan = jobDefinition.FullScan;
            UrlPattern = jobDefinition.UrlPattern;
        }

        public JobDefinition ToJobDefinition()
        {
            return new JobDefinition(new JobDefinitionDto(
                AdjacencyXPath, AdjacencyAttribute, ResourceXPath, ResourceAttribute,
                ResourceRepoArgs, RootUrl, HttpRequestRetries, HttpRequestDelayBetweenRetries, WhatIf, FullScan, UrlPattern));
        }

        public JobDefinition ToJobDefinition(string? rootUrl)
        {
            return new JobDefinition(new JobDefinitionDto(
                AdjacencyXPath, AdjacencyAttribute, ResourceXPath, ResourceAttribute,
                ResourceRepoArgs, RootUrl, HttpRequestRetries, HttpRequestDelayBetweenRetries, WhatIf, FullScan, UrlPattern), rootUrl);
        }

        public string Id { get; set; } = null!;
        public string? RootUrl { get; set; }
        public string AdjacencyXPath { get; set; } = null!;
        public string AdjacencyAttribute { get; set; } = null!;
        public string ResourceXPath { get; set; } = null!;
        public string ResourceAttribute { get; set; } = null!;
        public IResourceRepositoryConfiguration ResourceRepoArgs { get; set; } = null!;
        public int HttpRequestRetries { get; set; }
        public TimeSpan HttpRequestDelayBetweenRetries { get; set; }
        public bool WhatIf { get; set; }
        public bool FullScan { get; set; }
        public string? UrlPattern { get; set; } = null!;
    }
}