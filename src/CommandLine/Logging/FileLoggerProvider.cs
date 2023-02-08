using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Scrap.CommandLine.Logging;

[ProviderAlias("File")]
public class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggingConfiguration _configuration;

    public FileLoggerProvider(IOptions<FileLoggingConfiguration> options)
    {
        _configuration = options.Value;

        if (!Directory.Exists(_configuration.FolderPath))
        {
            throw new ArgumentException($"The logging folder {_configuration.FolderPath} does not exist");
        }
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(_configuration);

    public void Dispose()
    {
    }
}
