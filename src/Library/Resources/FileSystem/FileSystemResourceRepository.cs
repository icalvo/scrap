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
        private readonly HttpHelper _httpHelper;
        private readonly string _destinationRootFolder;
        private readonly bool _whatIf;
        private readonly ILogger<FileSystemResourceRepository> _logger;

        public FileSystemResourceRepository(IDestinationProvider destinationProvider, HttpHelper httpHelper, string destinationRootFolder, bool whatIf, ILogger<FileSystemResourceRepository> logger)
        {
            _destinationProvider = destinationProvider;
            _httpHelper = httpHelper;
            _destinationRootFolder = destinationRootFolder;
            _whatIf = whatIf;
            _logger = logger;
        }

        public async Task UpsertResourceAsync(
            Uri resourceUrl,
            Page page)
        {
            var destinationPath = _destinationProvider.GetDestination(
                resourceUrl,
                _destinationRootFolder,
                page);

            _logger.LogInformation("GET {0}", resourceUrl);
            _logger.LogInformation("-> {0}", destinationPath);
            try
            {
                var directoryName = Path.GetDirectoryName(destinationPath);
                if (directoryName != null)
                {
                    Directory.CreateDirectory(directoryName);
                    if (!File.Exists(destinationPath))
                    {
                        if (!_whatIf) {
                            await _httpHelper.DownloadFileAsync(resourceUrl, destinationPath);
                        }
                        _logger.LogInformation(" OK!");
                    }
                    else
                    {
                        _logger.LogInformation(" Already there!");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex, ex.Message);
                throw;
            }
        }
    }
}