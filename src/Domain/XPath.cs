using System.Xml.XPath;

namespace Scrap;

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
        return IsHtml ? "html:" + _expression.Expression : _expression.Expression;
    }

    public static implicit operator XPath(string x) => new(x);

    public static implicit operator XPathExpression(XPath x) => x._expression;

    public static implicit operator XPath(XPathExpression x) => new(x);

}