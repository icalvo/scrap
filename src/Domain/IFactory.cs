namespace Scrap.Domain;

public interface IFactory<out TOut>
{
    public TOut Build();
}

public interface IFactory<in TIn, out TOut>
{
    public TOut Build(TIn job);
}

public interface IAsyncFactory<in TIn, TOut>
{
    public Task<TOut> Build(TIn job);
}
