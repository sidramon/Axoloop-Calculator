namespace Domain.Calculator.Operations.Functions.Random;

using Domain.Calculator.Random;
using Domain.Calculator.Values;

/// <summary>
/// rand() and rand(n) — registered twice under the name "rand", at arity 0 (a single
/// draw) and arity 1 (a vector of n draws), the same overload-by-arity pattern used
/// elsewhere (e.g. integral(f)/integral(f, a)).
/// </summary>
public sealed class RandFunction : IFunction
{
    private readonly IRandomSource _source;
    private readonly bool _hasCount;

    public RandFunction(IRandomSource source, bool hasCount = false)
    {
        _source = source;
        _hasCount = hasCount;
    }

    public string Name => "rand";
    public int Arity => _hasCount ? 1 : 0;
    public FunctionCategory Category => FunctionCategory.Random;
    public string Signature => _hasCount ? "rand(n)" : "rand()";

    public string Description => _hasCount
        ? "n uniform random reals in [0, 1), returned as a 1xn vector. n must be a " +
          "positive integer. Draws from the shared random source (see seed)."
        : "A single uniform random real in [0, 1), drawn from the shared random source " +
          "— see seed for how to make the sequence reproducible.";

    public IReadOnlyList<string> Examples => _hasCount
        ? new[]
        {
            "rand(5) → a 1x5 vector of reals in [0, 1)",
            "mean(rand(10000)) → approximately 0.5",
        }
        : new[]
        {
            "rand() → a real in [0, 1), e.g. 0.7423",
        };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (!_hasCount)
            return new NumberValue(_source.NextDouble());

        var n = FunctionArguments.RequireInteger(arguments[0], "rand");
        if (n <= 0)
            throw new InvalidOperationException("rand(n) requires a positive integer n.");

        var values = new double[n];
        for (var i = 0; i < n; i++) values[i] = _source.NextDouble();
        return MatrixValue.FromRow(values);
    }
}
