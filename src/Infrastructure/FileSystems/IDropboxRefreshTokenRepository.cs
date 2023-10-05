namespace Scrap.Infrastructure.FileSystems;

public interface IDropboxRefreshTokenRepository
{
    Task<string?> GetTokenAsync();
    Task WriteTokenAsync(string refreshToken);
}
