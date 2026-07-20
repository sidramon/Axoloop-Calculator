namespace Domain.Calculator.Operations;

using Domain.Calculator.Values;

internal static class ValueArithmetic
{
    public static Value Add(Value left, Value right) => (left, right) switch
    {
        (NumberValue a, NumberValue b) => new NumberValue(a.Number + b.Number),
        (MatrixValue a, MatrixValue b) => a.Add(b),
        _ => throw Incompatible("addition", left, right)
    };

    public static Value Subtract(Value left, Value right) => (left, right) switch
    {
        (NumberValue a, NumberValue b) => new NumberValue(a.Number - b.Number),
        (MatrixValue a, MatrixValue b) => a.Subtract(b),
        _ => throw Incompatible("subtraction", left, right)
    };

    public static Value Multiply(Value left, Value right) => (left, right) switch
    {
        (NumberValue a, NumberValue b) => new NumberValue(a.Number * b.Number),
        (MatrixValue a, NumberValue b) => a.Scale(b.Number),
        (NumberValue a, MatrixValue b) => b.Scale(a.Number),
        (MatrixValue a, MatrixValue b) => a.Multiply(b),
        _ => throw Incompatible("multiplication", left, right)
    };

    public static Value Divide(Value left, Value right) => (left, right) switch
    {
        (NumberValue a, NumberValue b) => b.Number == 0
            ? throw new DivideByZeroException("Division by zero.")
            : new NumberValue(a.Number / b.Number),
        (MatrixValue a, NumberValue b) => b.Number == 0
            ? throw new DivideByZeroException("Division by zero.")
            : a.Scale(1.0 / b.Number),
        _ => throw Incompatible("division", left, right)
    };

    private static InvalidOperationException Incompatible(string op, Value left, Value right) =>
        new($"Cannot apply {op} to {TypeName(left)} and {TypeName(right)}.");

    private static string TypeName(Value v) => v switch
    {
        NumberValue    => "number",
        BooleanValue   => "boolean",
        MatrixValue    => "matrix",
        FunctionValue  => "function",
        SolutionValue  => "solution",
        SymbolicValue  => "symbolic",
        _ => "value"
    };
}