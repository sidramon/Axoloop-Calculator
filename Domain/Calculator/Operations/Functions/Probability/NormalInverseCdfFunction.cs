namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class NormalInverseCdfFunction : IFunction
{
    public string Name => "norminv";
    public int Arity => 3;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "norminv(p, mu, sigma)";

    public string Description =>
        "The p-quantile of the normal distribution with mean mu and standard deviation " +
        "sigma — the x such that normcdf(x, mu, sigma) = p. Found by bisection on normcdf " +
        "rather than a closed form (none exists in terms of erf/gamma). p must be " +
        "strictly between 0 and 1; sigma must be positive.";

    public IReadOnlyList<string> Examples => new[]
    {
        "norminv(0.5, 0, 1) → 0  (the median of a standard normal is its mean)",
        "norminv(normcdf(1.5, 0, 1), 0, 1) → 1.5  (round-trips through normcdf)",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var p = FunctionArguments.RequireNumber(arguments[0], "norminv");
        var mu = FunctionArguments.RequireNumber(arguments[1], "norminv");
        var sigma = FunctionArguments.RequireNumber(arguments[2], "norminv");
        return new NumberValue(NormalDistribution.InverseCdf(p, mu, sigma));
    }
}
