namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class LogGammaFunction : IFunction
{
    public string Name => "lgamma";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "lgamma(x)";

    public string Description =>
        "Natural log of gamma(x), computed directly in log space rather than as " +
        "ln(gamma(x)) — gamma(x) itself overflows a double past x around 171, while " +
        "lgamma stays finite far beyond that. Used internally by the Poisson distribution " +
        "to keep lambda^k / k! from overflowing for large k. Same domain restriction as " +
        "gamma: throws at zero and negative integers.";

    public IReadOnlyList<string> Examples => new[]
    {
        "lgamma(5) → 3.1780538  (= ln(24))",
        "lgamma(200) → a finite value, unlike gamma(200) which overflows to Infinity",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(SpecialFunctions.LogGamma(FunctionArguments.RequireNumber(arguments[0], "lgamma")));
}
