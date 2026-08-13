namespace Domain.Calculator.Operations.Functions.Random;

using Domain.Calculator.Random;
using Domain.Calculator.Values;

/// <summary>
/// randbin(n, p) — a single draw from a binomial distribution, simulated directly as n
/// independent Bernoulli(p) trials rather than inverting binomcdf against a uniform
/// draw. Distinct from binompdf/binomcdf (Probability/), which describe the
/// distribution rather than sample from it.
/// </summary>
public sealed class RandBinFunction : IFunction
{
    private readonly IRandomSource _source;

    public RandBinFunction(IRandomSource source) => _source = source;

    public string Name => "randbin";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Random;
    public string Signature => "randbin(n, p)";

    public string Description =>
        "A single random draw from a binomial distribution: the number of successes " +
        "out of n independent trials, each succeeding with probability p. n must be a " +
        "positive integer, p must be between 0 and 1.";

    public IReadOnlyList<string> Examples => new[]
    {
        "randbin(10, 0.5) → an integer in [0, 10], e.g. 6",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var n = FunctionArguments.RequireInteger(arguments[0], "randbin");
        var p = FunctionArguments.RequireNumber(arguments[1], "randbin");
        if (n <= 0)
            throw new InvalidOperationException("randbin requires a positive integer n.");
        if (p < 0 || p > 1)
            throw new InvalidOperationException("randbin requires p between 0 and 1.");

        var successes = 0;
        for (var i = 0; i < n; i++)
            if (_source.NextDouble() < p) successes++;

        return new NumberValue(successes);
    }
}
