using System.Xml.XPath;

namespace Scrap.Domain;

public class XPath
{
    private readonly XPathExpression _expression;

    public XPath(string xpath)
    {
        const string htmlPrefix = "html:";
        if (xpath.StartsWith(htmlPrefix))
        {
            IsHtml = true;
            _expression = XPathExpression.Compile(xpath[htmlPrefix.Length..]);
        }
        else
        {
            _expression = XPathExpression.Compile(xpath);
        }
    }

    private XPath(XPathExpression xpath)
    {
        _expression = xpath;
    }

    public bool IsHtml { get; }

    public override string ToString() => (IsHtml ? "html:" : "") + _expression.Expression;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((XPath)obj);
    }

    private bool Equals(XPath other) =>
        _expression.Expression.Equals(other._expression.Expression) && IsHtml == other.IsHtml;

    public override int GetHashCode() => HashCode.Combine(_expression, IsHtml);

    public static implicit operator XPath(string x) => new(x);

    public static implicit operator XPathExpression(XPath x) => x._expression;

    public static implicit operator XPath(XPathExpression x) => new(x);
}
