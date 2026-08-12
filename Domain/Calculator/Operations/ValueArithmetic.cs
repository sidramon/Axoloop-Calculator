namespace Domain.Calculator.Operations;

using System.Numerics;
using Domain.Calculator.Values;

internal static class ValueArithmetic
{
    public static Value Add(Value left, Value right) => (left, right) switch
    {
        (NumberValue a, NumberValue b) => new NumberValue(a.Number + b.Number),
        (MatrixValue a, MatrixValue b) => a.Add(b),
        _ when TryPromoteToComplex(left, right, out var a, out var b) => ComplexValue.Of(a + b),
        _ => throw Incompatible("addition", left, right)
    };

    public static Value Subtract(Value left, Value right) => (left, right) switch
    {
        (NumberValue a, NumberValue b) => new NumberValue(a.Number - b.Number),
        (MatrixValue a, MatrixValue b) => a.Subtract(b),
        _ when TryPromoteToComplex(left, right, out var a, out var b) => ComplexValue.Of(a - b),
        _ => throw Incompatible("subtraction", left, right)
    };

    public static Value Multiply(Value left, Value right) => (left, right) switch
    {
        (NumberValue a, NumberValue b) => new NumberValue(a.Number * b.Number),
        (MatrixValue a, NumberValue b) => a.Scale(b.Number),
        (NumberValue a, MatrixValue b) => b.Scale(a.Number),
        (MatrixValue a, MatrixValue b) => a.Multiply(b),
        _ when TryPromoteToComplex(left, right, out var a, out var b) => ComplexValue.Of(a * b),
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
        _ when TryPromoteToComplex(left, right, out var a, out var b) => b == Complex.Zero
            ? throw new DivideByZeroException("Division by zero (complex divisor is 0 + 0i).")
            : ComplexValue.Of(a / b),
        _ => throw Incompatible("division", left, right)
    };

    /// <summary>
    /// Centralizes the "a real meeting a complex becomes complex" promotion rule for every
    /// binary operator, rather than repeating a real/complex branch in each one. Only
    /// matches when at least one side is genuinely complex — a pure real/real pair is
    /// always caught by the NumberValue/NumberValue pattern above first, so this never
    /// intercepts ordinary real arithmetic.
    /// </summary>
    private static bool TryPromoteToComplex(Value left, Value right, out Complex a, out Complex b)
    {
        var leftOk = TryToComplex(left, out a);
        var rightOk = TryToComplex(right, out b);
        return leftOk && rightOk;
    }

    internal static bool TryToComplex(Value v, out Complex c)
    {
        switch (v)
        {
            case ComplexValue cv:
                c = cv.ToComplex();
                return true;
            case NumberValue nv:
                c = new Complex(nv.Number, 0);
                return true;
            default:
                c = default;
                return false;
        }
    }

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
        ComplexValue   => "complex",
        _ => "value"
    };
}