using Scrap.Common;
using SharpX;

namespace Scrap.Domain;

public readonly struct NameOrRootUrl
{
    private readonly Union<(string Name, Uri RootUrl), Union<string, Uri>> _union;

    public NameOrRootUrl(Uri rootUrl) : this(null, rootUrl)
    {
    }

    public NameOrRootUrl(string? name, Uri? rootUrl = null)
    {
        _union = name == null
            ?
            rootUrl == null
                ? throw new Exception()
                : Union<(string Name, Uri RootUrl), Union<string, Uri>>.FromRight(Union<string, Uri>.FromRight(rootUrl))
            : rootUrl == null
                ? Union<(string Name, Uri RootUrl), Union<string, Uri>>.FromRight(Union<string, Uri>.FromLeft(name))
                : Union<(string Name, Uri RootUrl), Union<string, Uri>>.FromLeft((name, rootUrl));
    }

    public Maybe<TResult> MatchName<TResult>(Func<string, TResult> f) =>
        _union.Match(x => Maybe.Just(f(x.Name)), x => x.Match(y => Maybe.Just(f(y)), y => Maybe.Nothing<TResult>()));

    public Maybe<TResult> MatchRootUrl<TResult>(Func<Uri, TResult> f) =>
        _union.Match(x => Maybe.Just(f(x.RootUrl)), x => x.Match(_ => Maybe.Nothing<TResult>(), y => Maybe.Just(f(y))));

    public TResult MatchNameFirst<TResult>(Func<string, TResult> fn, Func<Uri, TResult> fru) =>
        _union.Match(x => fn(x.Name), x => x.Match(fn, fru));

    public static Maybe<NameOrRootUrl> Create(string? envName, Uri? uri)
    {
        if (envName != null || uri != null)
        {
            return new NameOrRootUrl(envName, uri).ToJust();
        }

        return Maybe.Nothing<NameOrRootUrl>();
    }

    public static implicit operator NameOrRootUrl(string name) => new(name);

    public static implicit operator NameOrRootUrl(Uri rootUrl) => new(rootUrl);

    public override string? ToString() => _union.ToString();
}
