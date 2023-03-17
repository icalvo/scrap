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

// ReSharper disable once UnusedType.Global -- Used when compiling expressions
public class TemplateDestinationProvider: IDestinationProvider
{
    private readonly IFileSystem _fileSystem;

    public TemplateDestinationProvider(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public Task ValidateAsync()
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

    private string ToPath(IEnumerable<string> parts)
    {
        return _fileSystem.Path.Combine(parts.ToArray());
    }
}
