namespace Domain.Calculator.Operations.Functions.Random;

using Domain.Calculator.Random;
using Domain.Calculator.Values;

/// <summary>randint(min, max) and randint(min, max, n) — same two-arity pattern as <see cref="RandFunction"/>.</summary>
public sealed class RandIntFunction : IFunction
{
    private readonly IRandomSource _source;
    private readonly bool _hasCount;

    public RandIntFunction(IRandomSource source, bool hasCount = false)
    {
        _source = source;
        _hasCount = hasCount;
    }

    public string Name => "randint";
    public int Arity => _hasCount ? 3 : 2;
    public FunctionCategory Category => FunctionCategory.Random;
    public string Signature => _hasCount ? "randint(min, max, n)" : "randint(min, max)";

    public string Description => _hasCount
        ? "n uniform random integers in [min, max] (both ends included), returned as a " +
          "1xn vector. Requires min <= max and a positive integer n."
        : "A single uniform random integer in [min, max] — both ends included. Requires " +
          "min <= max.";

    public IReadOnlyList<string> Examples => _hasCount
        ? new[] { "randint(1, 6, 10) → a 1x10 vector of dice rolls in [1, 6]" }
        : new[] { "randint(1, 6) → an integer in [1, 6], e.g. 4", "randint(1, 1) → 1  (a degenerate but valid range)" };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var min = FunctionArguments.RequireInteger(arguments[0], "randint");
        var max = FunctionArguments.RequireInteger(arguments[1], "randint");
        if (min > max)
            throw new InvalidOperationException($"randint requires min <= max (got min={min}, max={max}).");

        if (!_hasCount)
            return new NumberValue(_source.NextInt(min, max));

        var n = FunctionArguments.RequireInteger(arguments[2], "randint");
        if (n <= 0)
            throw new InvalidOperationException("randint(min, max, n) requires a positive integer n.");

        var values = new double[n];
        for (var i = 0; i < n; i++) values[i] = _source.NextInt(min, max);
        return MatrixValue.FromRow(values);
    }
}
