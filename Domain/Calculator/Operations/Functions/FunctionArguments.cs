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

    public static int RequireInteger(Value value, string function)
    {
        if (value is not NumberValue n)
            throw new InvalidOperationException($"{function} requires a number.");
        if (n.Number % 1 != 0)
            throw new InvalidOperationException($"{function} requires an integer.");
        return (int)n.Number;
    }

    public static FunctionValue RequireUnaryFunction(Value value, string function)
    {
        if (value is not FunctionValue f)
            throw new InvalidOperationException($"{function} requires a function as its first argument.");
        if (f.Arity != 1)
            throw new InvalidOperationException(
                $"{function} requires a single-parameter function, but '{f.Name}' takes {f.Arity}.");
        return f;
    }

    public static Func<double, double> AsNumericFunction(FunctionValue function) => x =>
    {
        var result = function.Invoke(new Value[] { new NumberValue(x) });
        if (result is not NumberValue number)
            throw new InvalidOperationException($"'{function.Name}' must return a number.");
        return number.Number;
    };
}