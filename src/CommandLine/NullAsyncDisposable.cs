class NullAsyncDisposable : IAsyncDisposable
{
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}