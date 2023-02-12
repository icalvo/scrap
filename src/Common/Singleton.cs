namespace Scrap.Common;

public static class Singleton<T>
{
    private static T? _item;
    private static readonly object Lock = new();

    public static T Get(Func<T> constructor)
    {
        lock (Lock)
        {
            _item ??= constructor();
        }

        return _item;
    }
}
