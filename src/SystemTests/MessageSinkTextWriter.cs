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
