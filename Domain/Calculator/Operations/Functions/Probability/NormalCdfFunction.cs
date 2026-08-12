namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class NormalCdfFunction : IFunction
{
    public string Name => "normcdf";
    public int Arity => 3;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "normcdf(x, mu, sigma)";

    public string Description =>
        "Cumulative distribution function of the normal distribution with mean mu and " +
        "standard deviation sigma, evaluated at x — P(X <= x). Computed as " +
        "0.5*(1+erf((x-mu)/(sigma*sqrt(2)))), so it inherits erf's ~1e-7 accuracy. sigma " +
        "must be positive.";

    public IReadOnlyList<string> Examples => new[]
    {
        "normcdf(0, 0, 1) → 0.5  (symmetric about the mean)",
        "normcdf(1, 0, 1) → 0.8413447  (one standard deviation above the mean)",
        "normcdf(2, 0, 1) → 0.9772499  (two standard deviations above the mean)",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var x = FunctionArguments.RequireNumber(arguments[0], "normcdf");
        var mu = FunctionArguments.RequireNumber(arguments[1], "normcdf");
        var sigma = FunctionArguments.RequireNumber(arguments[2], "normcdf");
        return new NumberValue(NormalDistribution.Cdf(x, mu, sigma));
    }
}
