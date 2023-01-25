using System.Text;
using Xunit.Abstractions;

namespace Scrap.Tests.Integration;

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
        if (value != null) _testOutputHelper.WriteLine(value);
    }
}
