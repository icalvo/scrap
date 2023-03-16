using Dropbox.Api;
using Dropbox.Api.Files;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Infrastructure.FileSystems;

public class DropboxFileSystem : IFileSystem
{
    private readonly DropboxClient _client;

    public DropboxFileSystem(DropboxClient client)
    {
        _client = client;
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

    public Task FileWriteAsync(string directoryName, string destinationPath, Stream stream)
    {
        return _client.Files.UploadAsync(
            destinationPath,
            WriteMode.Overwrite.Instance,
            body: stream);
    }

    public Task FileWriteAllTextAsync(string filePath, string content) =>
        _client.Files.UploadAsync(filePath, body: new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)));

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
        return
            splitPoint == -1
                ? destinationPath
                : destinationPath[..destinationPath.LastIndexOf("/", StringComparison.Ordinal)];
    }

    public string PathNormalizeFolderSeparator(string path) =>
        path.Replace('\\', '/');

    public bool IsReadOnly => false;
}
