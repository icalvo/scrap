using Microsoft.Extensions.DependencyInjection;
using Scrap.Domain;

namespace Scrap.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFactory<TIn, TOut, TFactoryImpl>(this IServiceCollection container)
        where TFactoryImpl : class, IFactory<TIn, TOut>
    {
        container.AddSingleton<IFactory<TIn, TOut>, TFactoryImpl>();

        return container;
    }

    public static IServiceCollection AddFactory<TIn1, TIn2, TOut, TFactoryImpl>(this IServiceCollection container)
        where TFactoryImpl : class, IFactory<TIn1, TIn2, TOut>
    {
        container.AddSingleton<IFactory<TIn1, TIn2, TOut>, TFactoryImpl>();

        return container;
    }

    public static IServiceCollection AddFactory<TIn, TOut, TFactoryImpl>(
        this IServiceCollection container,
        Func<IServiceProvider, TFactoryImpl> builder)
        where TFactoryImpl : class, IFactory<TIn, TOut>
    {
        container.AddSingleton<IFactory<TIn, TOut>>(builder);

        return container;
    }

    public static IServiceCollection AddAsyncFactory<TIn, TOut, TFactoryImpl>(this IServiceCollection container)
        where TFactoryImpl : class, IAsyncFactory<TIn, TOut>
    {
        container.AddSingleton<IAsyncFactory<TIn, TOut>, TFactoryImpl>();

        return container;
    }

    public static IServiceCollection AddOptionalFactory<TIn, TOut, TFactoryImpl>(this IServiceCollection container)
        where TFactoryImpl : class, IFactory<TOut>, IFactory<TIn, TOut>
    {
        container.AddSingleton<TFactoryImpl>();
        container.AddSingleton<IFactory<TOut>>(sp => sp.GetRequiredService<TFactoryImpl>());
        container.AddSingleton<IFactory<TIn, TOut>>(sp => sp.GetRequiredService<TFactoryImpl>());

        return container;
    }
}
