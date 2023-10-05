using Moq;
using Scrap.Domain.Resources.FileSystem;
using Xunit;

namespace Scrap.Tests.Unit;

public class DirectoryToolsTests
{
    [Fact]
    public async Task CreateIfNotExistsAsync_WhenItDoesNotExist_Creates()
    {
        var rfsMock = new Mock<IRawFileSystem>(MockBehavior.Strict);
        rfsMock.Setup(x => x.DirectoryExistsAsync("/folder")).ReturnsAsync(false);
        rfsMock.Setup(x => x.DirectoryCreateAsync("/folder")).Returns(Task.CompletedTask);
        var sut = new DirectoryTools(rfsMock.Object);

        await sut.CreateIfNotExistAsync("/folder");

        rfsMock.Verify();
    }

    [Fact]
    public async Task CreateIfNotExistsAsync_WhenItExists_DoesNotCreate()
    {
        var rfsMock = new Mock<IRawFileSystem>(MockBehavior.Strict);
        rfsMock.Setup(x => x.DirectoryExistsAsync("/folder")).ReturnsAsync(true);
        var sut = new DirectoryTools(rfsMock.Object);

        await sut.CreateIfNotExistAsync("/folder");

        rfsMock.Verify();
    }
}
