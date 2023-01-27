namespace Scrap.Domain;

public interface IOptionalParameterFactory<in TIn, out TOut> : IFactory<TIn, TOut>, IFactory<TOut> where TIn : class
{
}
