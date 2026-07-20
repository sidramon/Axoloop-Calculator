namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class DerivFunction : IFunction
{
    public string Name => "deriv";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "deriv(f, x)";

    public string Description =>
        "Numerical derivative of f at x, computed by a centered finite-difference " +
        "approximation with a step scaled to the magnitude of x (a fixed step would be " +
        "catastrophic on very large or very small values). This is an approximation, not a " +
        "symbolic result: expect a handful fewer correct digits than the calculator's usual " +
        "precision for a smooth function, and larger error near kinks or discontinuities. " +
        "f must be a function value of arity 1. See also deriv(f), which returns the " +
        "derivative as a callable function instead of a single value, and deriv(f, n, x), " +
        "which gives the nth derivative.";

    public IReadOnlyList<string> Examples => new[]
    {
        "f(x) := x^2 :: deriv(f, 3) → 6",
        "deriv(sin, 0) → 1",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var function = FunctionArguments.RequireUnaryFunction(arguments[0], "deriv");
        var x = FunctionArguments.RequireNumber(arguments[1], "deriv");
        var evaluate = FunctionArguments.AsNumericFunction(function);

        return new NumberValue(NumericalCalculus.Derivative(evaluate, x));
    }
}
