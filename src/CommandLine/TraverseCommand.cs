using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrap.Application;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;

namespace Scrap.CommandLine;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class TraverseCommand : AsyncCommandBase<TraverseSettings>
{
    public TraverseCommand(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem)
        : base(configuration, oAuthCodeGetter, fileSystem)
    {
    }

    protected override async Task<int> CommandExecuteAsync(TraverseSettings settings)
    {
        var serviceResolver = BuildServiceProviderWithoutConsole();
        var newJob = await BuildJobDtoAsync(serviceResolver, settings.Name, settings.RootUrl, settings.FullScan, false, true, true);
        if (newJob == null)
        {
            return 0;
        }

        var service = serviceResolver.GetRequiredService<ITraversalApplicationService>();
        await service.TraverseAsync(newJob).ForEachAsync(x => Console.WriteLine(x));

        return 0;
    }
}
