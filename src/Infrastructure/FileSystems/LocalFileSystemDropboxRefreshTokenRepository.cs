namespace Scrap.Infrastructure.FileSystems;

public class LocalFileSystemDropboxRefreshTokenRepository : IDropboxRefreshTokenRepository
{
    private readonly string _tokenFile;

    public LocalFileSystemDropboxRefreshTokenRepository(string tokenFile)
    {
        _tokenFile = tokenFile;
    }

    public async Task<string?> GetTokenAsync()
    {
        if (File.Exists(_tokenFile))
        {
            return await File.ReadAllTextAsync(_tokenFile);
        }

        return null;
    }

    public Task WriteTokenAsync(string refreshToken) => File.WriteAllTextAsync(_tokenFile, refreshToken);
}
