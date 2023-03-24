﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

internal sealed class ShowConfigSettings : SettingsBase
{
    public ShowConfigSettings(bool debug, bool verbose) : base(debug, verbose)
    {
    }
}

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class ShowConfigCommand : AsyncCommandBase<ShowConfigSettings>
{
    public ShowConfigCommand(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem)
        : base(configuration, oAuthCodeGetter, fileSystem)
    {
    }

    protected override Task<int> CommandExecuteAsync(ShowConfigSettings settings)
    {
        Debug.Assert(Configuration != null, nameof(Configuration) + " != null");
        var root = (IConfigurationRoot)Configuration;
        Console.WriteLine(root.GetDebugView());

        return Task.FromResult(0);
    }
}