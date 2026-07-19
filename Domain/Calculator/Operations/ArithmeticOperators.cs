namespace Domain.Calculator.Operations;

using Domain.Calculator.Values;

public sealed class AddOperator : IOperator
{
    public string Symbol => "+";
    public int Precedence => 4;
    public Associativity Associativity => Associativity.Left;
    public Value Apply(Value left, Value right) => ValueArithmetic.Add(left, right);
}

public sealed class SubtractOperator : IOperator
{
    public string Symbol => "-";
    public int Precedence => 4;
    public Associativity Associativity => Associativity.Left;
    public Value Apply(Value left, Value right) => ValueArithmetic.Subtract(left, right);
}

public sealed class MultiplyOperator : IOperator
{
    public string Symbol => "*";
    public int Precedence => 5;
    public Associativity Associativity => Associativity.Left;
    public Value Apply(Value left, Value right) => ValueArithmetic.Multiply(left, right);
}

public sealed class DivideOperator : IOperator
{
    public string Symbol => "/";
    public int Precedence => 5;
    public Associativity Associativity => Associativity.Left;
    public Value Apply(Value left, Value right) => ValueArithmetic.Divide(left, right);
}

public sealed class ModuloOperator : IOperator
{
    public string Symbol => "%";
    public int Precedence => 5;
    public Associativity Associativity => Associativity.Left;
    public Value Apply(Value left, Value right) => (left, right) switch
    {
        (NumberValue a, NumberValue b) => b.Number == 0
            ? throw new DivideByZeroException("Modulo by zero.")
            : new NumberValue(a.Number % b.Number),
        _ => throw new InvalidOperationException("Modulo requires numbers.")
    };
}

public sealed class PowerOperator : IOperator
{
    public string Symbol => "^";
    public int Precedence => 6;
    public Associativity Associativity => Associativity.Right;
    public Value Apply(Value left, Value right) => (left, right) switch
    {
        (NumberValue a, NumberValue b) => new NumberValue(Math.Pow(a.Number, b.Number)),
        _ => throw new InvalidOperationException("Power requires numbers.")
    };
}