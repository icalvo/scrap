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
            Directory.CreateDirectory(_configuration.FolderPath);
        }
    }
 
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_configuration);
    }
 
    public void Dispose()
    {
    }
}
