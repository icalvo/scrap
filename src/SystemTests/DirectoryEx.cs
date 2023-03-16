namespace Scrap.Tests.System;

public static class DirectoryEx
{
    public static void DeleteIfExists(string path, bool recursive = false)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive);
        }
    }
}
