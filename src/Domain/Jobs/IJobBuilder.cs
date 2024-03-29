﻿using Scrap.Domain.Sites;
using SharpX;

namespace Scrap.Domain.Jobs;

public interface IJobBuilder
{
    Task<Maybe<(Job job, string siteName)>> BuildJobAsync(
        Maybe<NameOrRootUrl> argNameOrRootUrl,
        bool? fullScan,
        bool? downloadAlways,
        bool? disableMarkingVisited,
        bool? disableResourceWrites);

    Job BuildJob(
        Site site,
        string? argRootUrl = null,
        string? envRootUrl = null,
        bool? fullScan = null,
        bool? downloadAlways = null,
        bool? disableMarkingVisited = null,
        bool? disableResourceWrites = null);
}
