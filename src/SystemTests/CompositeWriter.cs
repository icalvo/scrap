using System.Text;

namespace Scrap.Tests.System;

public class CompositeWriter : TextWriter
{
    private readonly IEnumerable<TextWriter> _childTextWriters;

    private CompositeWriter(IEnumerable<TextWriter> childTextWriters)
    {
        var textWriters = childTextWriters as TextWriter[] ?? childTextWriters.ToArray();
        if (textWriters.Select(x => x.Encoding).Distinct().Count() != 1)
        {
            throw new ArgumentException("Encodings are not the same", nameof(childTextWriters));
        }

        _childTextWriters = textWriters;
    }

    public static CompositeWriter Create(params TextWriter[] childTextWriters) => new CompositeWriter(childTextWriters);

    public CompositeWriter(IFormatProvider? formatProvider, IEnumerable<TextWriter> childTextWriters) : base(
        formatProvider)
    {
        _childTextWriters = childTextWriters;
    }

    public override void WriteLine(string? value)
    {
        foreach (var childTextWriter in _childTextWriters)
        {
            childTextWriter.WriteLine(value);
        }
    }

    public override Encoding Encoding => _childTextWriters.First().Encoding;
}
