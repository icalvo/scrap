using Scrap.Domain;

namespace Scrap.DependencyInjection;

public class EntityRegistry<T> : IEntityRegistry<T>
{
    internal T? RegisteredEntity { get; private set; }

    public void Register(T entity)
    {
        RegisteredEntity = entity;
    }
}
