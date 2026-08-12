namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class GammaFunction : IFunction
{
    public string Name => "gamma";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "gamma(x)";

    public string Description =>
        "Gamma function: generalizes the factorial to non-integers, with gamma(n) = " +
        "(n-1)! for a positive integer n. Used directly by choose/perm's underlying " +
        "combinatorics and by the Poisson distribution. Computed via the Lanczos " +
        "approximation (g=7, 9 terms) — accurate to about 1e-15, i.e. full double " +
        "precision. Undefined (throws) at zero and negative integers, where the true " +
        "gamma function has poles. Overflows to Infinity for x greater than roughly 171 — " +
        "use lgamma for larger arguments.";

    public IReadOnlyList<string> Examples => new[]
    {
        "gamma(5) → 24  (= 4!)",
        "gamma(0.5) → 1.7724539  (= sqrt(pi))",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(SpecialFunctions.Gamma(FunctionArguments.RequireNumber(arguments[0], "gamma")));
}
