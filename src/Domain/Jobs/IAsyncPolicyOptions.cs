namespace Scrap.Domain.Jobs;

public interface IAsyncPolicyOptions
{
    public int HttpRequestRetries { get; }
    public TimeSpan HttpRequestDelayBetweenRetries { get; }
}