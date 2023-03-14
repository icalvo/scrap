namespace Scrap.Common;

public interface ISingleOptionalParameterFactory<in TIn, out TOut> : IFactory<TIn, TOut>, IFactory<TOut>
    where TIn : class
{
}
