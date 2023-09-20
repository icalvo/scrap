using System.Diagnostics;

namespace Scrap.Common;

public static class AsyncLazy
{
    public static AsyncLazy<T> Create<T>(Func<Task<T>> func) => new(func);

    public static AsyncLazy<T> Create<T>(T instance) => new(instance);
}

public sealed class AsyncLazy<T>
{
    private readonly Func<Task<T>>? _func;
    private T? _value;

    public AsyncLazy(Func<Task<T>> func)
    {
        _func = func;
    }

    public AsyncLazy(T instance)
    {
        _value = instance;
    }

    public bool IsEvaluated => _value != null;

    public async Task<T> ValueAsync()
    {
        if (_value != null)
        {
            return _value;
        }

        Debug.Assert(_func != null);
        _value = await _func();

        return _value;
    }

    public static implicit operator AsyncLazy<T>(T instance) => new(instance);
}
