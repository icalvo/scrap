namespace Scrap.CommandLine;

internal interface ICommandSetup
{
    Type OptionsType { get; }
    Task ExecuteAsync(object options);
}
