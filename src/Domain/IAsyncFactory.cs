namespace Scrap.Domain;

public interface IAsyncFactory<in TIn, TOut>
{
    public Task<TOut> Build(TIn param);
}
