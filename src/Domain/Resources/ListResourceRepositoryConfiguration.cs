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

    public string ListPath { get; } = null!;

    public string RepositoryType => "list";

    public Task ValidateAsync(ILoggerFactory loggerFactory) => Task.CompletedTask;
}
