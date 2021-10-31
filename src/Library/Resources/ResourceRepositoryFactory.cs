using System;
using Microsoft.Extensions.Logging;
using Scrap.Pages;
using Scrap.Resources.FileSystem;

namespace Scrap.Resources
{
    public class ResourceRepositoryFactory : IResourceRepositoryFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public ResourceRepositoryFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IResourceRepository Build(string id, params string[] args)
        {
            switch (id)
            {
                case "filesystem":
                    string destinationRootFolder = args[0];
                    string destinationExpression = args[1];
                    bool whatIf = bool.Parse(args[2]);
                    var destinationProvider = DestinationProvider.CreateCompiled(
                        destinationExpression,
                        new Logger<DestinationProvider>(_loggerFactory));
                    return new FileSystemResourceRepository(
                        destinationProvider,
                        new HttpHelper(new Logger<HttpHelper>(_loggerFactory)),
                        destinationRootFolder,
                        whatIf,
                        new Logger<FileSystemResourceRepository>(_loggerFactory));
                default:
                    throw new ArgumentException($"Invalid resource repository type {id}", nameof(id));
            }
        }
    }
}