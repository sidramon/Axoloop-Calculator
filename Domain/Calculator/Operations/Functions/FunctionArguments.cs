namespace Domain.Calculator.Operations.Functions;

using Domain.Calculator.Values;

internal static class FunctionArguments
{
    public static int RequireSize(Value value, string function)
    {
        if (value is not NumberValue n)
            throw new InvalidOperationException($"{function} requires numeric dimensions.");
        if (n.Number % 1 != 0)
            throw new InvalidOperationException($"{function} dimensions must be integers.");
        return (int)n.Number;
    }

    public static MatrixValue RequireMatrix(Value value, string function)
    {
        if (value is not MatrixValue m)
            throw new InvalidOperationException($"{function} requires a matrix.");
        return m;
    }

    public static double RequireNumber(Value value, string function)
    {
        if (value is not NumberValue n)
            throw new InvalidOperationException($"{function} requires a number.");
        return n.Number;
    }
}