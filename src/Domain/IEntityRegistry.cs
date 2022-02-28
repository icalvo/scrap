namespace Scrap.Domain;

public interface IEntityRegistry<T>
{
    public T? RegisteredEntity { get; }
    void Register(T entity);
}
