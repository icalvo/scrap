using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Scrap.Tests.System;

public class MessageSinkTextWriter : TextWriter
{
    private readonly IMessageSink _messageSink;

    public MessageSinkTextWriter(IMessageSink messageSink)
    {
        _messageSink = messageSink;
    }

    public override Encoding Encoding { get; } = Encoding.Default;

    public override void WriteLine(string? value)
    {
        if (value != null)
        {
            _messageSink.OnMessage(new DiagnosticMessage(value));
        }
    }    
}

public class TestOutputHelperTextWriter : TextWriter
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TestOutputHelperTextWriter(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public override Encoding Encoding { get; } = Encoding.Default;

    public override void WriteLine(string? value)
    {
        if (value != null)
        {
            _testOutputHelper.WriteLine(value);
        }
    }
}
