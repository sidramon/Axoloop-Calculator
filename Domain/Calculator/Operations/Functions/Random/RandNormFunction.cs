namespace Domain.Calculator.Operations.Functions.Random;

using Domain.Calculator.Random;
using Domain.Calculator.Values;

/// <summary>randnorm(mu, sigma) and randnorm(mu, sigma, n) — same two-arity pattern as <see cref="RandFunction"/>.</summary>
public sealed class RandNormFunction : IFunction
{
    private readonly IRandomSource _source;
    private readonly bool _hasCount;

    public RandNormFunction(IRandomSource source, bool hasCount = false)
    {
        _source = source;
        _hasCount = hasCount;
    }

    public string Name => "randnorm";
    public int Arity => _hasCount ? 3 : 2;
    public FunctionCategory Category => FunctionCategory.Random;
    public string Signature => _hasCount ? "randnorm(mu, sigma, n)" : "randnorm(mu, sigma)";

    public string Description => _hasCount
        ? "n random draws from a normal distribution with mean mu and standard " +
          "deviation sigma, returned as a 1xn vector (Box-Muller transform). sigma must " +
          "be positive and n a positive integer."
        : "A single random draw from a normal distribution with mean mu and standard " +
          "deviation sigma (Box-Muller transform). sigma must be positive.";

    public IReadOnlyList<string> Examples => _hasCount
        ? new[] { "std(randnorm(0, 1, 10000)) → approximately 1" }
        : new[] { "randnorm(0, 1) → a real near 0, e.g. -0.42", "randnorm(100, 15) → a real near 100" };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var mu = FunctionArguments.RequireNumber(arguments[0], "randnorm");
        var sigma = FunctionArguments.RequireNumber(arguments[1], "randnorm");
        if (sigma <= 0)
            throw new InvalidOperationException("randnorm requires sigma > 0.");

        if (!_hasCount)
            return new NumberValue(_source.NextGaussian(mu, sigma));

        var n = FunctionArguments.RequireInteger(arguments[2], "randnorm");
        if (n <= 0)
            throw new InvalidOperationException("randnorm(mu, sigma, n) requires a positive integer n.");

        var values = new double[n];
        for (var i = 0; i < n; i++) values[i] = _source.NextGaussian(mu, sigma);
        return MatrixValue.FromRow(values);
    }
}
