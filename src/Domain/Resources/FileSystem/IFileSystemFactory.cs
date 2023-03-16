using Scrap.Common;

namespace Scrap.Domain.Resources.FileSystem;

public interface IFileSystemFactory : IAsyncFactory<bool?, IFileSystem>{}
