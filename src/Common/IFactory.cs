namespace Scrap.Common;

public interface IFactory<out TOut>
{
    public TOut Build();
}

public interface IFactory<in TIn, out TOut>
{
    public TOut Build(TIn param);
}

public interface IFactory<in TIn1, in TIn2, out TOut>
{
    public TOut Build(TIn1 param1, TIn2 param2);
}
