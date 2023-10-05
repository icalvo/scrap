namespace Scrap.CommandLine.Commands;

internal interface IVerb<TCommand, TOptions> where TCommand : class, IVerb<TCommand, TOptions>
{
    Task ExecuteAsync(TOptions settings);
}
