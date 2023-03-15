using Scrap.Common;
using Scrap.Domain.Resources.FileSystem;

namespace Scrap.Domain;

public interface IFileSystemFactory : IAsyncFactory<bool?, IFileSystem>{}
