﻿using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Infrastructure.FileSystems;

public class LocalFileSystem : IRawFileSystem
{
    public Task DirectoryCreateAsync(string path) => TaskExtensions.CompletedTask(() => Directory.CreateDirectory(path));

    public Task<bool> FileExistsAsync(string path) => Task.FromResult(File.Exists(path));

    public async Task FileWriteAsync(string destinationPath, Stream stream)
    {
        await using var outputStream = File.Open(destinationPath, FileMode.Create);
        await stream.CopyToAsync(outputStream);
    }

    public Task FileWriteAllTextAsync(string filePath, string content) =>
        File.WriteAllTextAsync(filePath, content);

    public Task<Stream> FileOpenReadAsync(string filePath) => Task.FromResult((Stream)File.OpenRead(filePath));
    public string PathCombine(string baseDirectory, string filePath) => Path.Combine(baseDirectory, filePath);

    public Task<string> FileReadAllTextAsync(string filePath) => File.ReadAllTextAsync(filePath);
    public string PathGetRelativePath(string relativeTo, string path) => Path.GetRelativePath(relativeTo, path);
    public string PathGetDirectoryName(string destinationPath) => Path.GetDirectoryName(destinationPath) ?? 
                                                                  throw new Exception("What??");
    public string PathNormalizeFolderSeparator(string path) => path;
    public bool IsReadOnly => false;

    public string DefaultGlobalUserConfigFile =>
        PathCombine(
            PathCombine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".scrap"),
            "scrap-user.json");
    public Task<bool> DirectoryExistsAsync(string path) => Task.FromResult(Directory.Exists(path));

    public string PathReplaceForbiddenChars(string path, string replacement = "") =>
        Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).Aggregate(
            path,
            (p, invalidChar) => p.Replace(invalidChar.ToString(), replacement));
}
