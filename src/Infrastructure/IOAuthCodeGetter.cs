namespace Scrap.Infrastructure;

public interface IOAuthCodeGetter
{
    Task<string?> GetAuthCodeAsync(Uri authorizeUri);
}
