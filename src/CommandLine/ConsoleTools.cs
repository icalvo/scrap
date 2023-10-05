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
}
