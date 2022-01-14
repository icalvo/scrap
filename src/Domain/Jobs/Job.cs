using System;
using Microsoft.Extensions.Logging;
using Scrap.JobDefinitions;
using Scrap.Resources;

namespace Scrap.Jobs;

public class Job
{
    public const int DefaultHttpRequestRetries = 5;
    public static readonly TimeSpan DefaultHttpRequestDelayBetweenRetries = TimeSpan.FromSeconds(1);

    public Job(NewJobDto dto)
    {
        Id = new JobId();
        AdjacencyXPath = dto.AdjacencyXPath == null ? null : new XPath(dto.AdjacencyXPath);
        ResourceXPath = new XPath(dto.ResourceXPath);
        ResourceRepoArgs = dto.ResourceRepository;
        RootUrl = new Uri(dto.RootUrl ?? throw new ArgumentException("Root URL must not be null", nameof(dto)));
        HttpRequestRetries = dto.HttpRequestRetries ?? DefaultHttpRequestRetries;
        HttpRequestDelayBetweenRetries = dto.HttpRequestDelayBetweenRetries ?? DefaultHttpRequestDelayBetweenRetries;
        WhatIf = dto.WhatIf ?? false;
        FullScan = dto.FullScan ?? false;
        DownloadAlways = dto.DownloadAlways ?? false;
        ResourceType = dto.ResourceType;
    }

    public void Log(ILogger logger, LogLevel logLevel = LogLevel.Debug)
    {
        logger.Log(logLevel, "Root URL: {RootUrl}", RootUrl);
        logger.Log(logLevel, "Adjacency XPath: {AdjacencyXPath}", AdjacencyXPath);
        logger.Log(logLevel, "Resource XPath: {ResourceXPath}", ResourceXPath);
        logger.Log(logLevel, "Resource repo args: {ResourceRepoArgs}", ResourceRepoArgs);
        logger.Log(logLevel, "What if flag: {WhatIf}", WhatIf);
        logger.Log(logLevel, "Full scan flag: {FullScan}", FullScan);
        logger.LogDebug("Resource Type: {ResourceType}", ResourceType);
    }


    public JobId Id { get; }
    public Uri RootUrl { get; }
    public XPath? AdjacencyXPath { get; }
    public XPath ResourceXPath { get; }
    public IResourceRepositoryConfiguration ResourceRepoArgs { get; }
    public int HttpRequestRetries { get; }
    public TimeSpan HttpRequestDelayBetweenRetries { get; }
    public bool WhatIf { get; }
    public bool FullScan { get; }
    public bool DownloadAlways { get; }
    public ResourceType ResourceType { get; }
}