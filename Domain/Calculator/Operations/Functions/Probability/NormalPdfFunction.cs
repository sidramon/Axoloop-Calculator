namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class NormalPdfFunction : IFunction
{
    public string Name => "normpdf";
    public int Arity => 3;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "normpdf(x, mu, sigma)";

    public string Description =>
        "Probability density of the normal distribution with mean mu and standard " +
        "deviation sigma, evaluated at x. sigma must be positive.";

    public IReadOnlyList<string> Examples => new[]
    {
        "normpdf(0, 0, 1) → 0.3989423  (standard normal peak)",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var x = FunctionArguments.RequireNumber(arguments[0], "normpdf");
        var mu = FunctionArguments.RequireNumber(arguments[1], "normpdf");
        var sigma = FunctionArguments.RequireNumber(arguments[2], "normpdf");
        return new NumberValue(NormalDistribution.Pdf(x, mu, sigma));
    }
}
