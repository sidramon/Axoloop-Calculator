namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class NdiffFunction : IFunction
{
    public string Name => "ndiff";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "ndiff(f, x)";

    public string Description =>
        "Numerical derivative of f at x, computed by a centered finite-difference " +
        "approximation with a step scaled to the magnitude of x (a fixed step would be " +
        "catastrophic on very large or very small values). This is an approximation, not a " +
        "symbolic result: expect a handful fewer correct digits than the calculator's usual " +
        "precision for a smooth function, and larger error near kinks or discontinuities. " +
        "f must be a function value of arity 1. See also ndiff(f), which returns the " +
        "derivative as a callable function instead of a single value, and ndiff(f, n, x), " +
        "which gives the nth derivative. For an exact, symbolic derivative of an expression " +
        "instead of a sampled function, see diff.";

    public IReadOnlyList<string> Examples => new[]
    {
        "f(x) := x^2 :: ndiff(f, 3) → 6",
        "ndiff(sin, 0) → 1",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var function = FunctionArguments.RequireUnaryFunction(arguments[0], "ndiff");
        var x = FunctionArguments.RequireNumber(arguments[1], "ndiff");
        var evaluate = FunctionArguments.AsNumericFunction(function);

        return new NumberValue(NumericalCalculus.Derivative(evaluate, x));
    }
}
