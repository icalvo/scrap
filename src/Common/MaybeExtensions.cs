using SharpX;

namespace Scrap.Common;

public static class MaybeExtensions
{
    public static TState Fold<T, TState>(this Maybe<T> first, Func<TState, T, TState> folder, TState state) =>
        first.Map(x => folder(state, x), () => state);

    public static Maybe<T> OrElse<T>(this Maybe<T> first, Maybe<T> ifNone) => first.Map(Maybe.Just, () => ifNone);

    public static Maybe<T> DoIfNothing<T>(this Maybe<T> maybe, Action ifNone)
    {
        if (maybe.IsNothing())
        {
            ifNone();
        }

        return maybe;
    }

    public static Maybe<T> Do<T>(this Maybe<T> maybe, Action<T> ifJust, Action ifNone)
    {
        maybe.Do(x => Unit.Do(() => ifJust(x)));
        if (maybe.IsNothing())
        {
            ifNone();
        }

        return maybe;
    }

    public static Result<T, TM> ToResult<T, TM>(this Maybe<T> maybe, TM errorMessage) =>
        maybe.Map(Result<T, TM>.Succeed, () => Result<T, TM>.FailWith(errorMessage));

    public static Maybe<T> ToMaybe2<T>(this T? obj) => obj == null ? Maybe.Nothing<T>() : Maybe.Just(obj);
    public static Maybe<T> ToMaybe3<T>(this T? obj) where T : struct => 
        obj == null ? Maybe.Nothing<T>() : Maybe.Just(obj.Value);

    public static T FromJust2<T>(this Maybe<T> obj, T ifNone) where T : class => 
        obj.FromJust(ifNone)!;
    public static T FromJust3<T>(this Maybe<T> obj, T ifNone) where T : struct => 
        obj.FromJust(ifNone);
}
