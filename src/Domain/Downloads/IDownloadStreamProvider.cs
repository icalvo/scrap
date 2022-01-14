using System;
using System.IO;
using System.Threading.Tasks;

namespace Scrap.Downloads;

public interface IDownloadStreamProvider
{
    Task<Stream> GetStreamAsync(Uri url);
}