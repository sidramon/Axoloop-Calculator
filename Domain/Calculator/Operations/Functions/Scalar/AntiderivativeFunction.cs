namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

/// <summary>
/// integral(f) and integral(f, a) — an antiderivative of f returned as a callable
/// function, rather than integral(f, a, b)'s single definite value. Registered twice
/// under the name "integral", at arity 1 (base point 0) and arity 2 (explicit base
/// point), mirroring how SolveForm covers two arities of "solve".
/// </summary>
public sealed class AntiderivativeFunction : IFunction
{
    // Fewer sub-intervals than IntegralFunction's 1000: the returned function reruns a
    // full quadrature on every call (e.g. once per point when plotted), so this trades
    // some precision for speed under the assumption of many evaluations rather than one.
    private const int Subintervals = 200;

    private readonly bool _hasExplicitBasePoint;

    public AntiderivativeFunction(bool hasExplicitBasePoint = false) => _hasExplicitBasePoint = hasExplicitBasePoint;

    public string Name => "integral";
    public int Arity => _hasExplicitBasePoint ? 2 : 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => _hasExplicitBasePoint ? "integral(f, a)" : "integral(f)";

    public string Description =>
        "Returns an antiderivative of f as a callable function: F(x) = the integral of f " +
        (_hasExplicitBasePoint ? "from a to x, using the given base point a" : "from 0 to x") +
        ". This is one antiderivative among infinitely many that differ by a constant — " +
        "specifically the one that vanishes at the base point. F(x) - F(y) is the definite " +
        "integral from y to x regardless of the base point, but F(x) alone is a convention, " +
        "not a canonical answer — changing the base point shifts F by a constant. Each call " +
        $"to F reruns a full quadrature from the base point, using fewer sub-intervals " +
        $"({Subintervals}) than integral(f, a, b)'s 1000, trading some precision for speed " +
        "since F is meant to be evaluated at many points (e.g. when plotted) rather than " +
        "once. f must be a function value of arity 1.";

    public IReadOnlyList<string> Examples => _hasExplicitBasePoint
        ? new[]
        {
            "f(x) := x^2 :: integral(f, 1)(2) → same value as integral(f, 1, 2)",
        }
        : new[]
        {
            "f(x) := x^2 :: integral(f)(1) → 0.3333... (the antiderivative vanishing at 0, at x = 1)",
            "f(x) := x^2 :: plot(integral(f), -5, 5) → traces an antiderivative of f",
        };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var function = FunctionArguments.RequireUnaryFunction(arguments[0], "integral");
        var basePoint = _hasExplicitBasePoint ? FunctionArguments.RequireNumber(arguments[1], "integral") : 0;
        var evaluate = FunctionArguments.AsNumericFunction(function);

        var antiderivative = NumericalCalculus.Antiderivative(evaluate, basePoint, Subintervals);

        var signature = $"integral({function.Name})(x)";
        return new FunctionValue("integral", 1, signature, args =>
        {
            var x = FunctionArguments.RequireNumber(args[0], "integral");
            return new NumberValue(antiderivative(x));
        });
    }
}
