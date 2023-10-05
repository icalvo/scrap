using SharpX;

namespace Scrap.Common;

public readonly struct Union<TLeft, TRight> where TLeft : notnull
    where TRight : notnull
{
    private readonly TLeft? _leftValue;
    private readonly TRight? _rightValue;
    private readonly EitherType _tag;

    private Union(TLeft? leftValue, TRight? rightValue, EitherType tag)
    {
        _leftValue = leftValue;
        _rightValue = rightValue;
        _tag = tag;
    }

    public static Union<TLeft, TRight> FromLeft(TLeft value) => new(value, default, EitherType.Left);

    public static Union<TLeft, TRight> FromRight(TRight value) => new(default, value, EitherType.Right);

    public TResult Match<TResult>(Func<TLeft, TResult> leftFunc, Func<TRight, TResult> rightFunc) =>
        _tag switch
        {
            EitherType.Right => rightFunc(_rightValue!),
            EitherType.Left => leftFunc(_leftValue!),
            _ => throw new InvalidOperationException()
        };

    public void Do(Action<TLeft> leftFunc, Action<TRight> rightFunc)
    {
        switch (_tag)
        {
            case EitherType.Right:
                rightFunc(_rightValue!);
                break;
            case EitherType.Left:
                leftFunc(_leftValue!);
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    public override string? ToString() => Match(l => l.ToString(), r => r.ToString());
}
