namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

/// <summary>
/// deriv(f, n, x) — the nth derivative of f at x. Distinct from <see cref="DerivFunction"/>
/// (arity 2, first derivative value) and <see cref="DerivativeFunction"/> (arity 1, first
/// derivative as a function). Deliberately not composed as nested deriv(deriv(...)) calls —
/// <see cref="NumericalCalculus.NthDerivative"/> uses a direct higher-order stencil instead,
/// since nesting first-derivative computations amplifies rounding error at every level.
/// </summary>
public sealed class NthDerivativeFunction : IFunction
{
    public string Name => "deriv";
    public int Arity => 3;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "deriv(f, n, x)";

    public string Description =>
        "The nth derivative of f at x (n = 2 for the second derivative, 3 for the third, " +
        "and so on), computed directly from a higher-order central-difference stencil — " +
        "never by nesting first-derivative computations, which would amplify rounding error " +
        "by roughly four orders of magnitude at every level and make anything past the " +
        "second derivative meaningless. Precision still degrades with n: expect noticeably " +
        "fewer correct digits at n = 2 than for deriv(f, x), and fewer still at n = 3 or 4. " +
        "Capped at n = 4 — beyond that, double-precision arithmetic can no longer tell the " +
        "result apart from noise. n must be a positive integer; n = 0 is rejected rather " +
        "than silently returning f(x). f must be a function value of arity 1.";

    public IReadOnlyList<string> Examples => new[]
    {
        "f(x) := x^3 :: deriv(f, 2, 2) → 12",
        "deriv(sin, 3, 0) → -1",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var function = FunctionArguments.RequireUnaryFunction(arguments[0], "deriv");
        var n = FunctionArguments.RequireInteger(arguments[1], "deriv");
        var x = FunctionArguments.RequireNumber(arguments[2], "deriv");
        var evaluate = FunctionArguments.AsNumericFunction(function);

        return new NumberValue(NumericalCalculus.NthDerivative(evaluate, x, n));
    }
}
