namespace Scrap.CommandLine;

internal interface ICommandSetup
{
    Type OptionsType { get; }
    Task<int> ExecuteAsync(object options);
}
