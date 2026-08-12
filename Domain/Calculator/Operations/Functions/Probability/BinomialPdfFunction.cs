namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class BinomialPdfFunction : IFunction
{
    public string Name => "binompdf";
    public int Arity => 3;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "binompdf(k, n, p)";

    public string Description =>
        "Probability mass P(X=k) of the binomial distribution: n independent trials, each " +
        "succeeding with probability p, exactly k successes. Computed exactly (up to " +
        "choose's double-precision limits) as choose(n,k) * p^k * (1-p)^(n-k). n must be a " +
        "non-negative integer, p must be between 0 and 1, and k must be an integer between " +
        "0 and n.";

    public IReadOnlyList<string> Examples => new[]
    {
        "binompdf(2, 4, 0.5) → 0.375  (2 heads in 4 fair coin flips)",
        "binompdf(0, 4, 0.5) → 0.0625",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var k = FunctionArguments.RequireInteger(arguments[0], "binompdf");
        var n = FunctionArguments.RequireInteger(arguments[1], "binompdf");
        var p = FunctionArguments.RequireNumber(arguments[2], "binompdf");
        return new NumberValue(BinomialDistribution.Pmf(k, n, p));
    }
}
