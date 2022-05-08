// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Scrap.Domain.Pages;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Domain.Resources.FileSystem.Extensions;

#pragma warning disable 1998

namespace Scrap.Resources.FileSystem;

// ReSharper disable once UnusedType.Global
public class TemplateDestinationProvider: IDestinationProvider
{
    public Task ValidateAsync(FileSystemResourceRepositoryConfiguration config)
    {
        return Task.CompletedTask;
    }

    public async Task<string> GetDestinationAsync(
        string rootFolder,
        IPage page,
        int pageIndex,
        Uri resourceUrl,
        int resourceIndex)
    {
        var x = new[]
        {
            ToArray(rootFolder),
            /* DestinationPattern */
        };
            
        return ToPath(x.Aggregate(Enumerable.Empty<string>(), Enumerable.Concat));
    }

    private string[] ToArray(string item)
    {
        return new[] { item };
    }

    private string[] ToArray(IEnumerable<string> items)
    {
        return items.ToArray();
    }

    private static string ToPath(IEnumerable<string> parts)
    {
        return Path.Combine(parts.ToArray());
    }
}
