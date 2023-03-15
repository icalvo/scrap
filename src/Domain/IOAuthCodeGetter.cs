namespace Scrap.Domain;

public interface IOAuthCodeGetter
{
    Task<string?> GetAuthCodeAsync(Uri authorizeUri);
}
