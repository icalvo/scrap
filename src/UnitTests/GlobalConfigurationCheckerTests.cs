using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Scrap.CommandLine;
using Scrap.Domain;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;
using Xunit;

namespace Scrap.Tests.Unit;

public class GlobalConfigurationCheckerTests
{
    [Fact]
    public async Task AsyncCommandBase_NoConfig_Exception()
    {
        var configMock = new Mock<IConfiguration>();
        var rfsMock = new Mock<IRawFileSystem>();

        configMock.Setup(x => x[ConfigKeys.GlobalUserConfigPath]).Returns((string?)null);
        rfsMock.Setup(x => x.DefaultGlobalUserConfigFile).Returns("/def/scrap-user.json");
        rfsMock.Setup(x => x.FileExistsAsync("/def/scrap-user.json")).ReturnsAsync(false);

        var mock = new GlobalConfigurationChecker(configMock.Object, new FileSystem(rfsMock.Object));

        var action = () => mock.EnsureGlobalConfigurationAsync();

        (await action.Should().ThrowAsync<ScrapException>()).WithMessage(
            "The tool is not properly configured; call 'scrap config'");
    }

    [Fact]
    public async Task AsyncCommandBase_DefaultConfigExists_AllKeysSet_Success()
    {
        var configMock = new Mock<IConfiguration>();
        var rfsMock = new Mock<IRawFileSystem>();

        configMock.Setup(x => x[ConfigKeys.GlobalUserConfigPath]).Returns((string?)null);
        configMock.Setup(x => x[ConfigKeys.Definitions]).Returns("/home/cfg/defs.json");
        configMock.Setup(x => x[ConfigKeys.Database]).Returns("/home/cfg/scrap.db");
        rfsMock.Setup(x => x.DefaultGlobalUserConfigFile).Returns("/def/scrap-user.json");
        rfsMock.Setup(x => x.FileExistsAsync("/def/scrap-user.json")).ReturnsAsync(true);
        rfsMock.Setup(x => x.PathGetDirectoryName("/def/scrap-user.json")).Returns("/def");

        var sut = new GlobalConfigurationChecker(configMock.Object, new FileSystem(rfsMock.Object));

        var action = () => sut.EnsureGlobalConfigurationAsync();

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AsyncCommandBase_DefaultConfigExists_SomeKeysNotSet_Exception()
    {
        var configMock = new Mock<IConfiguration>();
        var rfsMock = new Mock<IRawFileSystem>();

        configMock.Setup(x => x[ConfigKeys.GlobalUserConfigPath]).Returns((string?)null);
        configMock.Setup(x => x[ConfigKeys.Definitions]).Returns("/home/cfg/defs.json");
        configMock.Setup(x => x[ConfigKeys.Database]).Returns((string?)null);
        rfsMock.Setup(x => x.DefaultGlobalUserConfigFile).Returns("/def/scrap-user.json");
        rfsMock.Setup(x => x.FileExistsAsync("/def/scrap-user.json")).ReturnsAsync(true);
        rfsMock.Setup(x => x.PathGetDirectoryName("/def/scrap-user.json")).Returns("/def");

        var sut = new GlobalConfigurationChecker(configMock.Object, new FileSystem(rfsMock.Object));

        var action = () => sut.EnsureGlobalConfigurationAsync();

        (await action.Should().ThrowAsync<ScrapException>()).WithMessage(
            "The tool is not properly configured; call 'scrap config'");
    }

    [Fact]
    public async Task AsyncCommandBase_GloballySetConfigExists_AllKeysSet_Success()
    {
        var configMock = new Mock<IConfiguration>();
        var oAuthCodeGetterMock = new Mock<IOAuthCodeGetter>();
        var rfsMock = new Mock<IRawFileSystem>();

        configMock.Setup(x => x[ConfigKeys.GlobalUserConfigPath]).Returns("/home/cfg/scrap.json");
        configMock.Setup(x => x[ConfigKeys.Definitions]).Returns("/home/cfg/defs.json");
        configMock.Setup(x => x[ConfigKeys.Database]).Returns("/home/cfg/scrap.db");
        rfsMock.Setup(x => x.DefaultGlobalUserConfigFile).Returns("/home/cfg/scrap.json");
        rfsMock.Setup(x => x.FileExistsAsync("/home/cfg/scrap.json")).ReturnsAsync(true);
        rfsMock.Setup(x => x.PathGetDirectoryName("/home/cfg/scrap.json")).Returns("/home/cfg");

        var sut = new GlobalConfigurationChecker(configMock.Object, new FileSystem(rfsMock.Object));

        var action = () => sut.EnsureGlobalConfigurationAsync();

        await action.Should().NotThrowAsync();
    }
}
