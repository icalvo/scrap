using System;
using Microsoft.Extensions.Logging;
using Scrap.Downloads;
using Scrap.Resources;
using Scrap.Resources.FileSystem;

namespace Scrap.ResourceDownloaders
{
    public class ResourceProcessorFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public ResourceProcessorFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        public IResourceProcessor Build(IResourceProcessorConfiguration configuration,
            IDownloadStreamProvider downloadStreamProvider)
        {
            switch (configuration)
            {
                case FileSystemResourceProcessorConfiguration config:
                    var destinationProvider = CompiledDestinationProvider.CreateCompiled(
                        config.DestinationExpression,
                        new Logger<CompiledDestinationProvider>(_loggerFactory));
                    var repo = new FileSystemResourceRepository(
                        destinationProvider,
                        config.DestinationRootFolder,
                        new Logger<FileSystemResourceRepository>(_loggerFactory));
                    return repo.BuildProcessor(downloadStreamProvider, _loggerFactory);
                case ListResourceProcessorConfiguration config:
                    return new ListResourceProcessor();
                default:
                    throw new ArgumentException(
                        "Unknown resource processor config type: " + configuration.GetType().Name,
                        nameof(configuration));
            }
        }
    }
}