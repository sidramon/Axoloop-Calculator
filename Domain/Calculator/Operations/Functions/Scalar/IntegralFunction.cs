namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class IntegralFunction : IFunction
{
    public string Name => "integral";
    public int Arity => 3;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "integral(f, a, b)";

    public string Description =>
        "Numerical definite integral of f over [a, b], computed by composite Simpson's rule " +
        "over 1000 sub-intervals. This is an approximation, not a symbolic result, but is " +
        "substantially more accurate than a finite-difference derivative for a smooth " +
        "integrand. Simpson's rule assumes smoothness: a function with a discontinuity or " +
        "rapid oscillation inside [a, b] can produce a noticeably inaccurate result. Reversed " +
        "bounds negate the result, the standard convention: integral(f, b, a) = " +
        "-integral(f, a, b). f must be a function value of arity 1. See also integral(f) and " +
        "integral(f, a), which return an antiderivative as a callable function instead of a " +
        "single value.";

    public IReadOnlyList<string> Examples => new[]
    {
        "f(x) := x^2 :: integral(f, 0, 1) → 0.3333...",
        "integral(sin, 0, pi) → 2",
        "integral(sin, pi, 0) → -2",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var function = FunctionArguments.RequireUnaryFunction(arguments[0], "integral");
        var a = FunctionArguments.RequireNumber(arguments[1], "integral");
        var b = FunctionArguments.RequireNumber(arguments[2], "integral");
        var evaluate = FunctionArguments.AsNumericFunction(function);

        return new NumberValue(NumericalCalculus.Integral(evaluate, a, b));
    }
}
