using Microsoft.Extensions.Logging.Console;

namespace Scrap.Common.Logging;

public class CustomConsoleFormatterOptions : ConsoleFormatterOptions
{
    public bool EnableColors { get; set; }
}
