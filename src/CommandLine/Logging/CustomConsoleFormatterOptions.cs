using Microsoft.Extensions.Logging.Console;

namespace Scrap.CommandLine.Logging;

public class CustomConsoleFormatterOptions : ConsoleFormatterOptions
{
    public bool EnableColors { get; set; }
}
