using System;
using System.Diagnostics.CodeAnalysis;
using Scrap.JobDefinitions;
using Scrap.Resources;

namespace Scrap.Jobs;

public class NewJobDto
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private NewJobDto()
    {
    }

    public NewJobDto(
        JobDefinitionDto jobDefinition,
        string? rootUrl,
        bool? whatIf,
        bool? fullScan,
        IResourceRepositoryConfiguration? configuration,
        bool? downloadAlways)
        : this(
            jobDefinition.AdjacencyXPath,
            jobDefinition.ResourceXPath,
            configuration ?? jobDefinition.ResourceRepository,
            rootUrl ?? jobDefinition.RootUrl ?? throw new ArgumentException("No root URL provided"),
            jobDefinition.HttpRequestRetries,
            jobDefinition.HttpRequestDelayBetweenRetries,
            whatIf,
            fullScan,
            downloadAlways,
            jobDefinition.ResourceType ?? default(ResourceType))
    {}

    public NewJobDto(
        string? adjacencyXPath,
        string resourceXPath,
        IResourceRepositoryConfiguration resourceRepository,
        string rootUrl,
        int? httpRequestRetries,
        TimeSpan? httpRequestDelayBetweenRetries,
        bool? whatIf,
        bool? fullScan,
        bool? downloadAlways,
        ResourceType resourceType)
    {
        AdjacencyXPath = adjacencyXPath;
        ResourceType = resourceType;
        ResourceXPath = resourceXPath;
        ResourceRepository = resourceRepository;
        RootUrl = rootUrl;
        HttpRequestRetries = httpRequestRetries;
        HttpRequestDelayBetweenRetries = httpRequestDelayBetweenRetries;
        WhatIf = whatIf;
        FullScan = fullScan;
        DownloadAlways = downloadAlways;
    }

    public string? AdjacencyXPath { get; }
    public string ResourceXPath { get; } = null!;
    public IResourceRepositoryConfiguration ResourceRepository { get; } = null!;
    public string RootUrl { get; } = null!;
    public int? HttpRequestRetries { get; }
    public TimeSpan? HttpRequestDelayBetweenRetries { get; }
    public bool? WhatIf { get; }
    public bool? FullScan { get; }
    public bool? DownloadAlways { get; }
    public ResourceType ResourceType { get; }
}