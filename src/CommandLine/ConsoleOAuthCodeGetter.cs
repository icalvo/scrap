using System.Diagnostics;
using Scrap.DependencyInjection.Factories;
using Scrap.Domain;

namespace Scrap.CommandLine;

internal class ConsoleOAuthCodeGetter : IOAuthCodeGetter
{
    public Task<string?> GetAuthCodeAsync(Uri authorizeUri)
    {
        Process.Start(new ProcessStartInfo { FileName = authorizeUri.ToString(), UseShellExecute = true });
        Console.Write("Dropbox auth code: ");
        var code = Console.ReadLine();
        return Task.FromResult(code);
    }
}
