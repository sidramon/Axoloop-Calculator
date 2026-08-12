namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class BinomialCdfFunction : IFunction
{
    public string Name => "binomcdf";
    public int Arity => 3;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "binomcdf(k, n, p)";

    public string Description =>
        "Cumulative probability P(X<=k) of the binomial distribution, summing binompdf " +
        "from 0 to k. Same validation as binompdf: n a non-negative integer, p between 0 " +
        "and 1, k an integer between 0 and n.";

    public IReadOnlyList<string> Examples => new[]
    {
        "binomcdf(2, 4, 0.5) → 0.6875",
        "binomcdf(4, 4, 0.5) → 1  (k = n always sums to 1)",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var k = FunctionArguments.RequireInteger(arguments[0], "binomcdf");
        var n = FunctionArguments.RequireInteger(arguments[1], "binomcdf");
        var p = FunctionArguments.RequireNumber(arguments[2], "binomcdf");
        return new NumberValue(BinomialDistribution.Cdf(k, n, p));
    }
}
