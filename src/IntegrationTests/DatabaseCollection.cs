using Xunit;

namespace Scrap.Tests;

[CollectionDefinition("Tool setup collection")]
public class DatabaseCollection : ICollectionFixture<ToolSetupFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}