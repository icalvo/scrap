using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Scrap.Domain.Resources;

public class ListResourceRepositoryConfiguration : IResourceRepositoryConfiguration
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private ListResourceRepositoryConfiguration()
    {
    }

    public ListResourceRepositoryConfiguration(string listPath)
    {
        ListPath = listPath;
    }

    public string ListPath { get; private set; } = null!;

    public Task ValidateAsync(ILoggerFactory loggerFactory)
    {
        return Task.CompletedTask;
    }

    public string Type => "list";
}
