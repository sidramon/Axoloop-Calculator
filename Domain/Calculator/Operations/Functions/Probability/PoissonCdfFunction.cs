namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class PoissonCdfFunction : IFunction
{
    public string Name => "poisscdf";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "poisscdf(k, lambda)";

    public string Description =>
        "Cumulative probability P(X<=k) of the Poisson distribution with rate lambda, " +
        "summing poisspdf from 0 to k. Same validation as poisspdf: lambda positive, k a " +
        "non-negative integer.";

    public IReadOnlyList<string> Examples => new[]
    {
        "poisscdf(0, 3) → 0.0497871",
        "poisscdf(3, 3) → 0.6472319",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var k = FunctionArguments.RequireInteger(arguments[0], "poisscdf");
        var lambda = FunctionArguments.RequireNumber(arguments[1], "poisscdf");
        return new NumberValue(PoissonDistribution.Cdf(k, lambda));
    }
}
