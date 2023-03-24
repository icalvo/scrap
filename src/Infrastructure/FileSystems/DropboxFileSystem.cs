using System.Net;
using System.Text;
using Dropbox.Api;
using Dropbox.Api.Files;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Infrastructure.FileSystems;

public interface IDropboxRefreshTokenRepository
{
    Task<string?> GetTokenAsync();
    Task WriteTokenAsync(string refreshToken);
}

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

public class DropboxFileSystem : IRawFileSystem
{
    private readonly DropboxClient _client;

    public static async Task<DropboxFileSystem> CreateAsync(string appKey, IDropboxRefreshTokenRepository tokenRepository, IOAuthCodeGetter getter)
    {
        return new DropboxFileSystem(await GetDropboxClientAsync(appKey, tokenRepository, getter));
    }

    public DropboxFileSystem(DropboxClient client)
    {
        _client = client;
    }

    public async Task DirectoryCreateAsync(string path)
    {
        try
        {
            await _client.Files.CreateFolderV2Async(path);
        }
        catch (ApiException<CreateFolderError> ex)
        {
            var directoryExists = ex.ErrorResponse is CreateFolderError.Path x && x.Value.IsConflict;
            if (!directoryExists) throw;
        }
    }

    public async Task<bool> FileExistsAsync(string path)
    {
        try
        {
            var _ = await _client.Files.GetMetadataAsync(path);
            return true;
        }
        catch
        {
            // ignored
        }

        return false;
    }

    public Task FileWriteAsync(string destinationPath, Stream stream)
    {
        return _client.Files.UploadAsync(destinationPath, WriteMode.Overwrite.Instance, body: stream);
    }

    public Task FileWriteAllTextAsync(string filePath, string content) =>
        _client.Files.UploadAsync(filePath, body: new MemoryStream(Encoding.UTF8.GetBytes(content)));

    public async Task<Stream> FileOpenReadAsync(string filePath) =>
        await (await _client.Files.DownloadAsync(filePath)).GetContentAsStreamAsync();

    public string PathCombine(string baseDirectory, string filePath) =>
        baseDirectory.TrimEnd('/') + "/" + filePath.TrimStart('/');

    public async Task<string> FileReadAllTextAsync(string filePath) =>
        await (await _client.Files.DownloadAsync(filePath)).GetContentAsStringAsync();

    public string PathGetRelativePath(string relativeTo, string path) =>
        path.StartsWith(relativeTo) ? path[relativeTo.Length..] : path;

    public string PathGetDirectoryName(string destinationPath)
    {
        var splitPoint = destinationPath.LastIndexOf("/", StringComparison.Ordinal);
        return splitPoint == -1
            ? destinationPath
            : destinationPath[..destinationPath.LastIndexOf("/", StringComparison.Ordinal)];
    }

    public string PathNormalizeFolderSeparator(string path) => path.Replace('\\', '/');

    public bool IsReadOnly => false;
    public string DefaultGlobalUserConfigFile => PathCombine("/.scrap", "scrap-user.json");
    public Task<bool> DirectoryExistsAsync(string path) => FileExistsAsync(path);


    private static async Task<DropboxClient> GetDropboxClientAsync(
        string appKey,
        IDropboxRefreshTokenRepository tokenRepository,
        IOAuthCodeGetter getter)
    {
        string? existingRefreshToken = await tokenRepository.GetTokenAsync();
        

        var (refreshToken, client) = await GetDropboxClientAuxAsync(appKey, existingRefreshToken, getter);

        if (existingRefreshToken != refreshToken)
        {
            await tokenRepository.WriteTokenAsync(refreshToken);
        }

        return client;
    }

    private static async Task<(string refreshToken, DropboxClient client)> GetDropboxClientAuxAsync(
        string appKey,
        string? existingRefreshToken,
        IOAuthCodeGetter getter)
    {
        if (existingRefreshToken == null)
        {
            return await GetByAuth();
        }

        var dropboxClient = new DropboxClient(existingRefreshToken, appKey);
        try
        {
            if (await dropboxClient.RefreshAccessToken(null))
            {
                return (existingRefreshToken, dropboxClient);
            }
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode >= HttpStatusCode.InternalServerError)
            {
                throw;
            }
        }

        return await GetByAuth();

        async Task<(string refreshToken, DropboxClient client)> GetByAuth()
        {
            var codeVerifier = DropboxOAuth2Helper.GeneratePKCECodeVerifier();
            var codeChallenge = DropboxOAuth2Helper.GeneratePKCECodeChallenge(codeVerifier);

            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(
                oauthResponseType: OAuthResponseType.Code,
                clientId: appKey,
                redirectUri: (string?)null,
                state: null,
                tokenAccessType: TokenAccessType.Offline,
                scopeList: null,
                includeGrantedScopes: IncludeGrantedScopes.None,
                codeChallenge: codeChallenge);
            var code = await getter.GetAuthCodeAsync(authorizeUri);
            OAuth2Response tokenResult = await DropboxOAuth2Helper.ProcessCodeFlowAsync(
                code: code,
                appKey: appKey,
                codeVerifier: codeVerifier,
                redirectUri: null);
            var client = new DropboxClient(
                appKey: appKey,
                oauth2AccessToken: tokenResult.AccessToken,
                oauth2RefreshToken: tokenResult.RefreshToken,
                oauth2AccessTokenExpiresAt: tokenResult.ExpiresAt ?? default(DateTime));
            return (tokenResult.RefreshToken, client);
        }
    }
}
