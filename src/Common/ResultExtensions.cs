using SharpX;

namespace Scrap.Common;

public static class ResultExtensions
{
    public static TState Fold<T, TState, TM>(this Result<T, TM> result, Func<TState, T, TState> folder, TState state) =>
        result.ToMaybe().Map(x => folder(state, x), () => state);

    public static Result<T, TM> OrElse<T, TM>(this Result<T, TM> result, Result<T, TM> ifNone) =>
        result.ToMaybe().Map(Result<T, TM>.Succeed, () => ifNone);

    public static Result<T, TM> DoIfSuccess<T, TM>(this Result<T, TM> result, Action<T> ifOk)
    {
        result.Match(
            (s, _) => ifOk(s),
            _ => {});

        return result;
    }

    public static TOut Map<TIn, TOut, TM>(this Result<TIn, TM> result,
        Func<TIn, TOut> ifOk, Func<IEnumerable<TM>, TOut> ifNone)
    {
        return result.Either(
            (s, _) => ifOk(s),
            ifNone);
    }

    public static Result<TIn, TM> AddFailMessage<TIn, TM>(this Result<TIn, TM> result,
        TM message)
    {
        return result.Either(
            (_, _) => result,
            m => new Bad<TIn, TM>(m.Append(message)));
    }

    public static Result<T, TM> DoIfFail<T, TM>(this Result<T, TM> result, Action<IEnumerable<TM>> ifNone)
    {
        result.Match(
            (_, _) => {},
            ifNone);

        return result;
    }

    public static Result<T, Unit> ToUnitResult<T>(this T obj) => Result<T, Unit>.Succeed(obj);
}
