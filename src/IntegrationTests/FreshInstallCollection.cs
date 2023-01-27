using Xunit;

namespace Scrap.Tests.Integration;

[CollectionDefinition(nameof(FreshInstallCollection))]
public class FreshInstallCollection : ICollectionFixture<FreshInstallSetupFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
