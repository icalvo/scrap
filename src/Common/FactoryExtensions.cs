namespace Scrap.Common;

public static class FactoryExtensions
{
    public static TOut Build<TIn1, TIn2, TOut>(
        this IFactory<TIn1, TIn2?, TOut> factory,
        TIn1 param1,
        TIn2? param2 = null) where TIn2 : class
    {
        return factory.Build(param1, param2);
    }

    public static Task<TOut> BuildAsync<TIn, TOut>(
        this IAsyncFactory<TIn?, TOut> factory,
        TIn? param = default)
    {
        return factory.BuildAsync(param);
    }
}
