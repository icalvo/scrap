using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Scrap.CommandLine;
using Scrap.Domain;
using Scrap.Domain.Resources.FileSystem;
using Scrap.Infrastructure;
using Spectre.Console.Cli;
using Xunit;

namespace Scrap.Tests.Unit;

public class AsyncCommandBaseTests
{
    [Fact]
    public async Task AsyncCommandBase_NoConfig_Exception()
    {
        var configMock = new Mock<IConfiguration>();
        var oAuthCodeGetterMock = new Mock<IOAuthCodeGetter>();
        var rfsMock = new Mock<IRawFileSystem>();

        configMock.Setup(x => x[ConfigKeys.GlobalUserConfigPath]).Returns((string?)null);
        rfsMock.Setup(x => x.DefaultGlobalUserConfigFile).Returns("/def/scrap-user.json");
        rfsMock.Setup(x => x.FileExistsAsync("/def/scrap-user.json")).ReturnsAsync(false);

        var mock = new TestCommand(configMock.Object, oAuthCodeGetterMock.Object, new FileSystem(rfsMock.Object));


        var settings = new TestSettings(false, true);

        var action = () => mock.ExecuteAsync(
            new CommandContext(Mock.Of<IRemainingArguments>(), "lolailo", null),
            settings);

        (await action.Should().ThrowAsync<ScrapException>()).WithMessage(
            "The tool is not properly configured; call 'scrap config'");
    }

    [Fact]
    public async Task AsyncCommandBase_DefaultConfigExists_AllKeysSet_Success()
    {
        var configMock = new Mock<IConfiguration>();
        var oAuthCodeGetterMock = new Mock<IOAuthCodeGetter>();
        var rfsMock = new Mock<IRawFileSystem>();

        configMock.Setup(x => x[ConfigKeys.GlobalUserConfigPath]).Returns((string?)null);
        configMock.Setup(x => x[ConfigKeys.Definitions]).Returns("/home/cfg/defs.json");
        configMock.Setup(x => x[ConfigKeys.Database]).Returns("/home/cfg/scrap.db");
        rfsMock.Setup(x => x.DefaultGlobalUserConfigFile).Returns("/def/scrap-user.json");
        rfsMock.Setup(x => x.FileExistsAsync("/def/scrap-user.json")).ReturnsAsync(true);
        rfsMock.Setup(x => x.PathGetDirectoryName("/def/scrap-user.json")).Returns("/def");

        var sut = new TestCommand(configMock.Object, oAuthCodeGetterMock.Object, new FileSystem(rfsMock.Object));

        var settings = new TestSettings(false, true);

        var action = () => sut.ExecuteAsync(
            new CommandContext(Mock.Of<IRemainingArguments>(), "lolailo", null),
            settings);

        await action.Should().NotThrowAsync();
        sut.ReceivedSettings.Should().BeSameAs(settings);
    }

    [Fact]
    public async Task AsyncCommandBase_DefaultConfigExists_SomeKeysNotSet_Exception()
    {
        var configMock = new Mock<IConfiguration>();
        var oAuthCodeGetterMock = new Mock<IOAuthCodeGetter>();
        var rfsMock = new Mock<IRawFileSystem>();

        configMock.Setup(x => x[ConfigKeys.GlobalUserConfigPath]).Returns((string?)null);
        configMock.Setup(x => x[ConfigKeys.Definitions]).Returns("/home/cfg/defs.json");
        configMock.Setup(x => x[ConfigKeys.Database]).Returns((string?)null);
        rfsMock.Setup(x => x.DefaultGlobalUserConfigFile).Returns("/def/scrap-user.json");
        rfsMock.Setup(x => x.FileExistsAsync("/def/scrap-user.json")).ReturnsAsync(true);
        rfsMock.Setup(x => x.PathGetDirectoryName("/def/scrap-user.json")).Returns("/def");

        var mock = new TestCommand(configMock.Object, oAuthCodeGetterMock.Object, new FileSystem(rfsMock.Object));

        var settings = new TestSettings(false, true);

        var action = () => mock.ExecuteAsync(
            new CommandContext(Mock.Of<IRemainingArguments>(), "lolailo", null),
            settings);

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

        var sut = new TestCommand(configMock.Object, oAuthCodeGetterMock.Object, new FileSystem(rfsMock.Object));

        var settings = new TestSettings(false, true);

        var action = () => sut.ExecuteAsync(
            new CommandContext(Mock.Of<IRemainingArguments>(), "lolailo", null),
            settings);

        await action.Should().NotThrowAsync();
        sut.ReceivedSettings.Should().BeSameAs(settings);
    }

    private class TestSettings : SettingsBase
    {
        public TestSettings(bool debug, bool verbose) : base(debug, verbose)
        {
        }
    }

    private class TestCommand : AsyncCommandBase<SettingsBase>
    {
        public TestCommand(IConfiguration configuration, IOAuthCodeGetter oAuthCodeGetter, IFileSystem fileSystem) :
            base(configuration, oAuthCodeGetter, fileSystem)
        {
        }

        public SettingsBase? ReceivedSettings { get; private set; }

        protected override Task<int> CommandExecuteAsync(SettingsBase settings)
        {
            ReceivedSettings = settings;
            return Task.FromResult(0);
        }
    }
}
