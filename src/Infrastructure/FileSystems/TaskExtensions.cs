namespace Scrap.Infrastructure.FileSystems;

public static class TaskExtensions
{
    public static Task CompletedTask(Action action)
    {
        action();
        return Task.CompletedTask;
    }
}
