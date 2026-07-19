namespace Domain.Calculator.Operations;

using Domain.Calculator.Values;

public sealed class FactorialOperator : IUnaryOperator
{
    public string Symbol => "!";

    public Value Apply(Value operand)
    {
        if (operand is not NumberValue n)
            throw new InvalidOperationException("Factorial requires a number.");
        if (n.Number < 0 || n.Number % 1 != 0)
            throw new InvalidOperationException("Factorial requires a non-negative integer.");

        double result = 1;
        for (var i = 2; i <= (int)n.Number; i++) result *= i;
        return new NumberValue(result);
    }
}