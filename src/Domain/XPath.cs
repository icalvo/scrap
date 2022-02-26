using System.Xml.XPath;

namespace Scrap.Domain;

public class XPath
{
    private readonly XPathExpression _expression;
    public bool IsHtml { get; }

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

    public XPath(XPathExpression xpath)
    {
        _expression = xpath;
    }

    public override string ToString()
    {
        return (IsHtml ? "html:" : "") + _expression.Expression;
    }

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

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((XPath)obj);
    }

    private bool Equals(XPath other)
    {
        return _expression.Expression.Equals(other._expression.Expression) && IsHtml == other.IsHtml;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_expression, IsHtml);
    }

    public static implicit operator XPath(string x) => new(x);

    public static implicit operator XPathExpression(XPath x) => x._expression;

    public static implicit operator XPath(XPathExpression x) => new(x);

}
