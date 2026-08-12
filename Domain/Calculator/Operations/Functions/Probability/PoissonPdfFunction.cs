namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class PoissonPdfFunction : IFunction
{
    public string Name => "poisspdf";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "poisspdf(k, lambda)";

    public string Description =>
        "Probability mass P(X=k) of the Poisson distribution with rate lambda: the " +
        "probability of exactly k events when events occur independently at an average " +
        "rate of lambda per interval. Computed in log space via lgamma (k! = " +
        "gamma(k+1)) rather than lambda^k / k! directly, avoiding overflow for large k. " +
        "lambda must be positive; k must be a non-negative integer.";

    public IReadOnlyList<string> Examples => new[]
    {
        "poisspdf(0, 3) → 0.0497871  (no events when 3 are expected on average)",
        "poisspdf(3, 3) → 0.2240418  (exactly the average count)",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var k = FunctionArguments.RequireInteger(arguments[0], "poisspdf");
        var lambda = FunctionArguments.RequireNumber(arguments[1], "poisspdf");
        return new NumberValue(PoissonDistribution.Pmf(k, lambda));
    }
}
