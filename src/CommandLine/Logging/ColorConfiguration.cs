using Microsoft.Extensions.Logging;

namespace Scrap.CommandLine.Logging;

public class ColorConfiguration
{
    public ConsoleColor? Trace { get; set; }
    public ConsoleColor? Debug { get; set; }
    public ConsoleColor? Information { get; set; }
    public ConsoleColor? Warning { get; set; }
    public ConsoleColor? Error { get; set; }
    public ConsoleColor? Critical { get; set; }

    public ConsoleColor? ColorFor(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.Trace => Trace,
            LogLevel.Debug => Debug,
            LogLevel.Information => Information,
            LogLevel.Warning => Warning,
            LogLevel.Error => Error,
            LogLevel.Critical => Critical,
            LogLevel.None => null,
            _ => null
        };
}
