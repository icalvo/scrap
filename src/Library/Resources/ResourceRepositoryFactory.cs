using System;
using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Pages;
using Scrap.Resources.FileSystem;

namespace Scrap.Resources
{
    public class ResourceRepositoryFactory : IResourceRepositoryFactory
    {
        private readonly ResourceDownloaderFactory _resourceDownloaderFactory;
        private readonly ILoggerFactory _loggerFactory;

        public ResourceRepositoryFactory(ResourceDownloaderFactory resourceDownloaderFactory, ILoggerFactory loggerFactory)
        {
            _resourceDownloaderFactory = resourceDownloaderFactory;
            _loggerFactory = loggerFactory;
        }

        public IResourceRepository Build(IAsyncPolicy httpPolicy, IResourceRepositoryConfiguration args, bool whatIf)
        {
            switch (args)
            {
                case FileSystemResourceRepositoryConfiguration config:
                    var destinationProvider = CompiledDestinationProvider.CreateCompiled(
                        config.DestinationExpression,
                        new Logger<CompiledDestinationProvider>(_loggerFactory));
                    return new FileSystemResourceRepository(
                        destinationProvider,
                        _resourceDownloaderFactory.Build(httpPolicy),
                        config.DestinationRootFolder,
                        whatIf,
                        new Logger<FileSystemResourceRepository>(_loggerFactory));
                default:
                    throw new ArgumentException($"Invalid resource repository type {args}", nameof(args));
            }
        }
    }
}