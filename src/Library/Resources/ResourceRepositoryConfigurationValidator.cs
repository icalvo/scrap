using System;
using Microsoft.Extensions.Logging;
using Polly;
using Scrap.Resources.FileSystem;

namespace Scrap.Resources
{
    public class ResourceRepositoryConfigurationValidator : IResourceRepositoryConfigurationValidator
    {
        private readonly ILoggerFactory _loggerFactory;

        public ResourceRepositoryConfigurationValidator(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void Validate(IResourceRepositoryConfiguration args)
        {
            switch (args)
            {
                case FileSystemResourceRepositoryConfiguration config:
                    _ = CompiledDestinationProvider.CreateCompiled(
                        config.DestinationExpression,
                        new Logger<CompiledDestinationProvider>(_loggerFactory));
                    break;
                default:
                    throw new ArgumentException($"Invalid resource repository type {args}", nameof(args));
            }
        }
    }
}