using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Scrap.Resources;

namespace Scrap.Jobs
{
    public class Job
    {
        private static readonly Regex AttributeRegex = new("/@[a-z-_]+$", RegexOptions.Compiled);
        private static readonly Regex FunctionRegex = new(@"/[a-z-_]+\(\)$", RegexOptions.Compiled);

        public const int DefaultHttpRequestRetries = 5;
        public const string DefaultAdjacencyAttribute = "href";
        public static readonly TimeSpan DefaultHttpRequestDelayBetweenRetries = TimeSpan.FromSeconds(1);

        public Job(NewJobDto dto)
        {
            Id = new JobId();
            AdjacencyXPath = dto.AdjacencyXPath;
            AdjacencyAttribute = dto.AdjacencyAttribute ?? DefaultAdjacencyAttribute;
            ResourceXPath = dto.ResourceXPath;
            ResourceAttribute = dto.ResourceAttribute;
            ResourceRepoArgs = dto.ResourceRepoArgs;
            RootUrl = new Uri(dto.RootUrl ?? throw new ArgumentException("Root URL must not be null", nameof(dto)));
            HttpRequestRetries = dto.HttpRequestRetries ?? DefaultHttpRequestRetries;
            HttpRequestDelayBetweenRetries = dto.HttpRequestDelayBetweenRetries ?? DefaultHttpRequestDelayBetweenRetries;
            WhatIf = dto.WhatIf ?? false;
            FullScan = dto.FullScan ?? false;
        }

        public Job(JobDto dto)
        {
            Id = dto.Id;
            
            var split = AttributeRegex.Split(dto.AdjacencyXPath, 2);
            if (split.Length >= 2)
            {
                AdjacencyXPath = split[0];
                AdjacencyAttribute = split[1];
            }

            split = FunctionRegex.Split(dto.AdjacencyXPath, 2);
            if (split.Length >= 2)
            {
                AdjacencyXPath = split[0];
                AdjacencyAttribute = split[1];
            }

            AdjacencyXPath = dto.AdjacencyXPath;
            AdjacencyAttribute = dto.AdjacencyAttribute;
            ResourceXPath = dto.ResourceXPath;
            ResourceAttribute = dto.ResourceAttribute;
            ResourceRepoArgs = dto.ResourceRepoArgs;
            RootUrl = new Uri(dto.RootUrl ?? throw new ArgumentException("Root URL must not be null", nameof(dto)));
            HttpRequestRetries = dto.HttpRequestRetries;
            HttpRequestDelayBetweenRetries = dto.HttpRequestDelayBetweenRetries;
            WhatIf = dto.WhatIf;
            FullScan = dto.FullScan;
        }

        public Job(JobDto dto, string? rootUrl)
            : this(dto with { RootUrl = rootUrl ?? dto.RootUrl })
        {
        }

        public JobDto ToDto()
        {
            return new JobDto(
                Id,
                AdjacencyXPath,
                AdjacencyAttribute,
                ResourceXPath,
                ResourceAttribute,
                ResourceRepoArgs,
                RootUrl.AbsoluteUri,
                HttpRequestRetries,
                HttpRequestDelayBetweenRetries,
                WhatIf,
                FullScan);            
        }

        public void Log(ILogger logger)
        {
            logger.LogDebug("Root URL: {RootUrl}", RootUrl);
            logger.LogDebug("Adjacency X-Path: {AdjacencyXPath}", AdjacencyXPath);
            logger.LogDebug("Adjacency attribute: {AdjacencyAttribute}", AdjacencyAttribute);
            logger.LogDebug("Resource X-Path: {ResourceXPath}", ResourceXPath);
            logger.LogDebug("Resource attribute: {ResourceAttribute}", ResourceAttribute);
            logger.LogDebug("Resource repo args: {ResourceRepoArgs}", ResourceRepoArgs);
            logger.LogDebug("What if flag: {WhatIf}", WhatIf);
            logger.LogDebug("Full scan flag: {FullScan}", FullScan);
        }

        public JobId Id { get; }
        public Uri RootUrl { get; }
        public string AdjacencyXPath { get; }
        public string AdjacencyAttribute { get; }
        public string ResourceXPath { get; }
        public string ResourceAttribute { get; }
        public IResourceRepositoryConfiguration ResourceRepoArgs { get; }
        public int HttpRequestRetries { get; }
        public TimeSpan HttpRequestDelayBetweenRetries { get; }
        public bool WhatIf { get; }
        public bool FullScan { get; }
    }
}
