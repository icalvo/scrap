using System.Net;
using Dropbox.Api;
using Microsoft.Extensions.Logging;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure.FileSystems;

namespace Scrap.Infrastructure.Factories;

public class FileSystemFactory : IFileSystemFactory
{
    private static IRawFileSystem? _instance;

    private readonly IOAuthCodeGetter _authCodeGetter;
    private readonly IFileSystem _tokenFileSystem;
    private readonly ILogger<FileSystemFactory> _logger;

    public FileSystemFactory(
        IOAuthCodeGetter authCodeGetter,
        string fileSystemType,
        IFileSystem tokenFileSystem,
        ILogger<FileSystemFactory> logger)
    {
        _tokenFileSystem = tokenFileSystem;
        _logger = logger;
        _authCodeGetter = authCodeGetter;
        FileSystemType = fileSystemType;
    }

    public FileSystemFactory(IOAuthCodeGetter authCodeGetter, string fileSystemType, ILogger<FileSystemFactory> logger)
    {
        var tokenFileSystem = new FileSystem(new LocalFileSystem());
        _tokenFileSystem = tokenFileSystem;
        _authCodeGetter = authCodeGetter;
        FileSystemType = fileSystemType;
        _logger = logger;
    }

    public string FileSystemType { get; }

    public async Task<IFileSystem> BuildAsync(bool? readOnly)
    {
        _instance ??= await BuildRawFileSystem();
        return new FileSystem(readOnly ?? false ? new FileSystemReadOnlyWrapper(_instance) : _instance);
    }

    private async Task<IRawFileSystem> BuildRawFileSystem()
    {
        var normalizedFileSystemType = FileSystemType.ToLowerInvariant();

        if (normalizedFileSystemType != "local")
        {
            _logger.LogInformation("{FileSystemType} filesystem will be used!", FileSystemType.ToUpperInvariant());
        }

        switch (normalizedFileSystemType)
        {
            case "dropbox":
            {
                var appKey = "0lemimx20njvqt2";
                var tokenFile = _tokenFileSystem.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".scrap",
                    "dropboxtoken.txt");
                var repo = new LocalFileSystemDropboxRefreshTokenRepository(tokenFile);
                return await DropboxFileSystem.CreateAsync(appKey, repo, _authCodeGetter);
            }
            case "local":
                return new LocalFileSystem();
            default:
                throw new Exception($"Unknown filesystem type: {FileSystemType}");
        }
    }
}
