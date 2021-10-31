using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scrap.Pages;

namespace Scrap.Resources.FileSystem
{
    public class FileSystemResourceRepository : IResourceRepository
    {
        private readonly IDestinationProvider _destinationProvider;
        private readonly IResourceDownloader _resourceDownloader;
        private readonly string _destinationRootFolder;
        private readonly bool _whatIf;
        private readonly ILogger<FileSystemResourceRepository> _logger;

        public FileSystemResourceRepository(IDestinationProvider destinationProvider, IResourceDownloader resourceDownloader, string destinationRootFolder, bool whatIf, ILogger<FileSystemResourceRepository> logger)
        {
            _destinationProvider = destinationProvider;
            _resourceDownloader = resourceDownloader;
            _destinationRootFolder = destinationRootFolder;
            _whatIf = whatIf;
            _logger = logger;
        }

        public async Task UpsertResourceAsync(
            Uri resourceUrl,
            Page page)
        {
            var destinationPath = await _destinationProvider.GetDestinationAsync(
                resourceUrl,
                _destinationRootFolder,
                page);

            _logger.LogInformation("GET {ResourceUrl}", resourceUrl);
            _logger.LogInformation("-> {DestinationPath}", destinationPath);

            var directoryName = Path.GetDirectoryName(destinationPath);
            if (directoryName != null)
            {
                Directory.CreateDirectory(directoryName);
                if (!File.Exists(destinationPath))
                {
                    if (!_whatIf)
                    {
                        await using var outputStream = File.Open(destinationPath, FileMode.Create);
                        await _resourceDownloader.DownloadFileAsync(resourceUrl, outputStream);
                    }
                    _logger.LogInformation(" OK!");
                }
                else
                {
                    _logger.LogInformation(" Already there!");
                }
            }
        }
    }
}