namespace Scrap.Common;

public interface IAsyncFactory<in TIn, TOut>
{
    public Task<TOut> BuildAsync(TIn param);
}

public interface IAsyncFactory<in TIn1, in TIn2, TOut>
{
    public Task<TOut> BuildAsync(TIn1 param1, TIn2 param2);
}
