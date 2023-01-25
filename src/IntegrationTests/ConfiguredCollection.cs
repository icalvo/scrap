using Xunit;

namespace Scrap.Tests;

[CollectionDefinition(nameof(ConfiguredCollection))]
public class ConfiguredCollection : ICollectionFixture<ConfiguredFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
