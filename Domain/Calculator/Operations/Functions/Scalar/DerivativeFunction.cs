namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

/// <summary>
/// deriv(f) — the first derivative of f returned as a callable function, rather than
/// deriv(f, x)'s single value at one point. Distinct from <see cref="DerivFunction"/>
/// (arity 2, value form) and <see cref="NthDerivativeFunction"/> (arity 3, nth derivative).
/// </summary>
public sealed class DerivativeFunction : IFunction
{
    public string Name => "deriv";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "deriv(f)";

    public string Description =>
        "Returns the first derivative of f as a callable function value: deriv(f)(3) is " +
        "equivalent to deriv(f, 3). Useful for composing with other functions, or for " +
        "plotting a derivative directly, e.g. plot(deriv(f), -10, 10). Each invocation of " +
        "the returned function reruns a finite-difference computation (two evaluations of " +
        "f), so sampling it over many points — a plot typically evaluates hundreds to " +
        "thousands — calls f that many times over; this is acceptable but worth knowing " +
        "before tracing a slow f over a fine grid. Same precision characteristics as " +
        "deriv(f, x).";

    public IReadOnlyList<string> Examples => new[]
    {
        "f(x) := x^2 :: deriv(f)(3) → 6",
        "f(x) := x^2 :: plot(deriv(f), -10, 10) → traces the derivative of f",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var function = FunctionArguments.RequireUnaryFunction(arguments[0], "deriv");
        var evaluate = FunctionArguments.AsNumericFunction(function);

        var signature = $"deriv({function.Name})(x)";
        return new FunctionValue("deriv", 1, signature, args =>
        {
            var x = FunctionArguments.RequireNumber(args[0], "deriv");
            return new NumberValue(NumericalCalculus.Derivative(evaluate, x));
        });
    }
}
