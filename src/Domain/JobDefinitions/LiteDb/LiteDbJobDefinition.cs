using System;
using System.Diagnostics.CodeAnalysis;
using Scrap.Resources;

namespace Scrap.JobDefinitions.LiteDb
{
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global", Justification = "Setters used by LiteDB")]
    public class LiteDbJobDefinition
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Constructor used by LiteDB")]
        public LiteDbJobDefinition() {}

        public LiteDbJobDefinition(JobDefinition jobDefinition)
        {
            Id = jobDefinition.Id;
            Name = jobDefinition.Name;
            AdjacencyXPath = jobDefinition.AdjacencyXPath;
            AdjacencyAttribute = jobDefinition.AdjacencyAttribute;
            ResourceXPath = jobDefinition.ResourceXPath;
            ResourceAttribute = jobDefinition.ResourceAttribute;
            ResourceRepoArgs = jobDefinition.ResourceRepoArgs;
            RootUrl = jobDefinition.RootUrl;
            HttpRequestRetries = jobDefinition.HttpRequestRetries;
            HttpRequestDelayBetweenRetries = jobDefinition.HttpRequestDelayBetweenRetries;
            UrlPattern = jobDefinition.UrlPattern;
        }

        public JobDefinition ToJobDefinition()
        {
            return new JobDefinition(new JobDefinitionDto(
                Id, Name, AdjacencyXPath, AdjacencyAttribute, ResourceXPath, ResourceAttribute,
                ResourceRepoArgs, RootUrl, HttpRequestRetries, HttpRequestDelayBetweenRetries, UrlPattern));
        }

        public JobDefinition ToJobDefinition(string? rootUrl)
        {
            return new JobDefinition(new JobDefinitionDto(
                Id, Name, AdjacencyXPath, AdjacencyAttribute, ResourceXPath, ResourceAttribute,
                ResourceRepoArgs, RootUrl, HttpRequestRetries, HttpRequestDelayBetweenRetries, UrlPattern), rootUrl);
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? RootUrl { get; set; }
        public string AdjacencyXPath { get; set; } = null!;
        public string? AdjacencyAttribute { get; set; }
        public string ResourceXPath { get; set; } = null!;
        public string ResourceAttribute { get; set; } = null!;
        public IResourceRepositoryConfiguration ResourceRepoArgs { get; set; } = null!;
        public int? HttpRequestRetries { get; set; }
        public TimeSpan? HttpRequestDelayBetweenRetries { get; set; }
        public string? UrlPattern { get; set; }
    }
}
