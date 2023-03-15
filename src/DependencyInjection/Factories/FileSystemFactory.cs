using System.Net;
using Dropbox.Api;
using Scrap.Domain;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.DependencyInjection.Factories;

public class FileSystemFactory : IFileSystemFactory
{
    private readonly IOAuthCodeGetter _authCodeGetter;
    private readonly string _fileSystemType;

    public FileSystemFactory(IOAuthCodeGetter authCodeGetter, string fileSystemType)
    {
        _authCodeGetter = authCodeGetter;
        _fileSystemType = fileSystemType;
    }

    public async Task<IFileSystem> BuildAsync(bool? readOnly)
    {
        var appKey = "0lemimx20njvqt2";
        var tokenFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".scrap", "dropboxtoken.txt");
        IFileSystem fs =
            _fileSystemType.ToLowerInvariant() switch {
                "dropbox" => new DropboxFileSystem(await GetDropboxClientAsync(appKey, tokenFile)),
                "local" => new LocalFileSystem(),
                _ => throw new Exception($"Unknown filesystem type: {_fileSystemType}")
            };

        return readOnly ?? false ? new FileSystemReadOnlyWrapper(fs) : fs;
    }
    
    private async Task<DropboxClient> GetDropboxClientAsync(string appKey, string tokenFile)
    {
        string? existingRefreshToken = null;
        if (File.Exists(tokenFile))
        {
            existingRefreshToken = await File.ReadAllTextAsync(tokenFile);
        }

        var (refreshToken, client) = await GetDropboxClientAuxAsync(appKey, existingRefreshToken);

        if (existingRefreshToken != refreshToken)
        {
            await File.WriteAllTextAsync(tokenFile, refreshToken);
        }

        return client;
    }
    
    private async Task<(string refreshToken, DropboxClient client)> GetDropboxClientAuxAsync(string appKey, string? existingRefreshToken)
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

            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(oauthResponseType:OAuthResponseType.Code,
                clientId: appKey,
                redirectUri: (string?)null,
                state: null,
                tokenAccessType: TokenAccessType.Offline,
                scopeList: null,
                includeGrantedScopes: IncludeGrantedScopes.None,
                codeChallenge: codeChallenge);
            var code = await _authCodeGetter.GetAuthCodeAsync(authorizeUri);
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
