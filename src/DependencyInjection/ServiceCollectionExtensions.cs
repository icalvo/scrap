using Microsoft.Extensions.DependencyInjection;
using Scrap.Domain;

namespace Scrap.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOptionalFactory<TIn, TOut, TFactoryImpl>(this IServiceCollection container)
        where TFactoryImpl : class, IFactory<TOut>, IFactory<TIn, TOut>
    {
        container.AddSingleton<TFactoryImpl>();
        container.AddSingleton<IFactory<TOut>>(sp => sp.GetRequiredService<TFactoryImpl>());
        container.AddSingleton<IFactory<TIn, TOut>>(sp => sp.GetRequiredService<TFactoryImpl>());

        return container;
    }
}