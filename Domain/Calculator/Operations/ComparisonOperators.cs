namespace Domain.Calculator.Operations;

using Domain.Calculator.Values;

public sealed class EqualsOperator : IOperator
{
    public string Symbol => "=";
    public int Precedence => 3;
    public Associativity Associativity => Associativity.Left;
    public Value Apply(Value left, Value right) => Compare(left, right, (a, b) => a == b);

    internal static Value Compare(Value left, Value right, Func<double, double, bool> op)
    {
        if (left is NumberValue a && right is NumberValue b)
            return new BooleanValue(op(a.Number, b.Number));
        throw new InvalidOperationException("Comparison requires numbers.");
    }
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