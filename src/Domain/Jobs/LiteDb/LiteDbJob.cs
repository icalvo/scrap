using System;
using System.Diagnostics.CodeAnalysis;
using Scrap.Resources;

namespace Scrap.Jobs.LiteDb
{
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global", Justification = "Setters used by LiteDB")]
    public class LiteDbJob
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Constructor used by LiteDB")]
        public LiteDbJob() {}

        public LiteDbJob(Job job)
        {
            Id = job.Id;
            AdjacencyXPath = job.AdjacencyXPath;
            AdjacencyAttribute = job.AdjacencyAttribute;
            ResourceXPath = job.ResourceXPath;
            ResourceAttribute = job.ResourceAttribute;
            ResourceRepoArgs = job.ResourceRepoArgs;
            RootUrl = job.RootUrl;
            HttpRequestRetries = job.HttpRequestRetries;
            HttpRequestDelayBetweenRetries = job.HttpRequestDelayBetweenRetries;
            WhatIf = job.WhatIf;
            FullScan = job.FullScan;
        }

        public Job ToJob()
        {
            return new Job(new JobDto(
                Id, AdjacencyXPath, AdjacencyAttribute, ResourceXPath, ResourceAttribute,
                ResourceRepoArgs, RootUrl, HttpRequestRetries, HttpRequestDelayBetweenRetries, WhatIf, FullScan));
        }

        public Job ToJob(string? rootUrl)
        {
            return new Job(new JobDto(
                Id, AdjacencyXPath, AdjacencyAttribute, ResourceXPath, ResourceAttribute,
                ResourceRepoArgs, RootUrl, HttpRequestRetries, HttpRequestDelayBetweenRetries, WhatIf, FullScan), rootUrl);
        }

        public Guid Id { get; set; }
        public string RootUrl { get; set; } = null!;
        public string AdjacencyXPath { get; set; } = null!;
        public string AdjacencyAttribute { get; set; } = null!;
        public string ResourceXPath { get; set; } = null!;
        public string ResourceAttribute { get; set; } = null!;
        public IResourceProcessorConfiguration ResourceRepoArgs { get; set; } = null!;
        public int HttpRequestRetries { get; set; }
        public TimeSpan HttpRequestDelayBetweenRetries { get; set; }
        public bool WhatIf { get; set; }
        public bool FullScan { get; set; }
    }
}