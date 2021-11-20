using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Scrap.Resources
{
    public class ListResourceRepositoryConfiguration : IResourceRepositoryConfiguration
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private ListResourceRepositoryConfiguration()
        {
        }

        public ListResourceRepositoryConfiguration(string listPath)
        {
            ListPath = listPath;
        }

        public string ListPath { get; private set; } = null!;

        public void Validate(ILoggerFactory loggerFactory)
        {
        }

        public string Type => "list";
    }
}
