using System.Reflection;

namespace Scrap.Tests.Integration;

public static class TaskExtensions
{
    public static object? Result(this Task t)
    {
        var type = t.GetType();
        return type.InvokeMember(
            nameof(Task<object>.Result),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
            null,
            t,
            null);
    }
}
