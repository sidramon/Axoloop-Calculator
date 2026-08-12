namespace Domain.Calculator.Operations;

using Domain.Calculator.Values;

public sealed class EqualsOperator : IOperator
{
    public string Symbol => "=";
    public int Precedence => 3;
    public Associativity Associativity => Associativity.Left;
    public Value Apply(Value left, Value right) => Compare(left, right, EqualityOp);

    /// <summary>
    /// Real numbers as before. Any pair involving a <see cref="ComplexValue"/> routes here
    /// too: equality (identified by reference to <see cref="EqualityOp"/>) compares both
    /// components within tolerance, while the three ordering operators reject the pair
    /// outright as soon as either side has a non-negligible imaginary part — complex numbers
    /// have no total order. A <see cref="ComplexValue"/> with a negligible imaginary part
    /// can't actually occur from ordinary evaluation — construction always reduces it to a
    /// real <see cref="NumberValue"/> first — but a directly-constructed one (bypassing
    /// <see cref="ComplexValue.Of(double, double)"/>) is still handled correctly rather than
    /// spuriously rejected: its real part is compared like any other real value.
    /// </summary>
    internal static Value Compare(Value left, Value right, Func<double, double, bool> op)
    {
        if (left is NumberValue a && right is NumberValue b)
            return new BooleanValue(op(a.Number, b.Number));

        if (left is ComplexValue || right is ComplexValue)
        {
            if (ReferenceEquals(op, EqualityOp))
                return new BooleanValue(ComplexEquals(left, right));

            if (HasNonNegligibleImaginaryPart(left) || HasNonNegligibleImaginaryPart(right))
                throw new InvalidOperationException("Complex numbers are not ordered.");

            var (leftReal, _) = AsComponents(left);
            var (rightReal, _) = AsComponents(right);
            return new BooleanValue(op(leftReal, rightReal));
        }

        throw new InvalidOperationException("Comparison requires numbers.");
    }

    private static readonly Func<double, double, bool> EqualityOp = (a, b) => a == b;

    private static bool HasNonNegligibleImaginaryPart(Value v) =>
        v is ComplexValue c && Math.Abs(c.Imaginary) >= ComplexValue.ReductionTolerance;

    private static bool ComplexEquals(Value left, Value right)
    {
        var (lr, li) = AsComponents(left);
        var (rr, ri) = AsComponents(right);
        return Math.Abs(lr - rr) < ComplexValue.ReductionTolerance
               && Math.Abs(li - ri) < ComplexValue.ReductionTolerance;
    }

    private static (double Real, double Imaginary) AsComponents(Value v) => v switch
    {
        ComplexValue c => (c.Real, c.Imaginary),
        NumberValue n => (n.Number, 0),
        _ => throw new InvalidOperationException("Comparison requires numbers.")
    };
}

public sealed class LessOrEqualOperator : IOperator
{
    public string Symbol => "<=";
    public int Precedence => 3;
    public Associativity Associativity => Associativity.Left;
    public Value Apply(Value left, Value right) => EqualsOperator.Compare(left, right, (a, b) => a <= b);
}

public sealed class GreaterOrEqualOperator : IOperator
{
    public string Symbol => ">=";
    public int Precedence => 3;
    public Associativity Associativity => Associativity.Left;
    public Value Apply(Value left, Value right) => EqualsOperator.Compare(left, right, (a, b) => a >= b);
}

public sealed class LessOperator : IOperator
{
    public string Symbol => "<";
    public int Precedence => 3;
    public Associativity Associativity => Associativity.Left;
    public Value Apply(Value left, Value right) => EqualsOperator.Compare(left, right, (a, b) => a < b);
}

public sealed class GreaterOperator : IOperator
{
    public string Symbol => ">";
    public int Precedence => 3;
    public Associativity Associativity => Associativity.Left;
    public Value Apply(Value left, Value right) => EqualsOperator.Compare(left, right, (a, b) => a > b);
}
