namespace Scrap.CommandLine.Commands;

internal class CommandException : Exception
{
    public int ReturnCode { get; }

    public CommandException(int returnCode, string message) : base(message)
    {
        ReturnCode = returnCode;
    }
}
