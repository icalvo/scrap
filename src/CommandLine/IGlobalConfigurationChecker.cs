namespace Scrap.CommandLine;

public interface IGlobalConfigurationChecker
{
    Task EnsureGlobalConfigurationAsync();
}
