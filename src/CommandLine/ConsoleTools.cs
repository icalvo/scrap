using Figgle;
using Scrap.CommandLine.Commands;

namespace Scrap.CommandLine;

public static class ConsoleTools
{
    public static IEnumerable<string> ConsoleInput()
    {
        while (Console.ReadLine() is { } line)
        {
            yield return line;
        }
    }

    public static void PrintHeader()
    {
        var version = VersionCommand.GetVersion();
        var currentColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(FiggleFonts.Doom.Render($"scrap {version}"));
        Console.WriteLine("Command line tool for generic web scrapping");
        Console.WriteLine();
        Console.ForegroundColor = currentColor;
    }
}
