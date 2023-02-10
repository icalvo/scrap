using Scrap.Domain;

namespace Scrap.DependencyInjection.Factories;

internal interface ISingleOptionalParameterFactory<in TIn, out TOut> : IFactory<TIn, TOut>, IFactory<TOut>
    where TIn : class
{
}
