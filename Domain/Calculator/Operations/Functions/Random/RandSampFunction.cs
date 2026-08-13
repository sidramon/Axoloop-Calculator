namespace Domain.Calculator.Operations.Functions.Random;

using Domain.Calculator.Random;
using Domain.Calculator.Values;

/// <summary>
/// randsamp(v, k) and randsamp(v, k, withReplacement) — registered twice like the other
/// two-arity random functions, but here the extra argument is a mode switch rather than
/// a count. The language has no boolean literal (same situation as variance/std's
/// population flag in Probability/), so withReplacement is a plain number: 0 (or the
/// two-argument form entirely) means without replacement, any nonzero value means with.
/// </summary>
public sealed class RandSampFunction : IFunction
{
    private readonly IRandomSource _source;
    private readonly bool _hasReplacementArgument;

    public RandSampFunction(IRandomSource source, bool hasReplacementArgument = false)
    {
        _source = source;
        _hasReplacementArgument = hasReplacementArgument;
    }

    public string Name => "randsamp";
    public int Arity => _hasReplacementArgument ? 3 : 2;
    public FunctionCategory Category => FunctionCategory.Random;
    public string Signature => _hasReplacementArgument ? "randsamp(v, k, withReplacement)" : "randsamp(v, k)";

    public string Description =>
        "k elements drawn at random from vector v, returned as a 1xk vector. WITHOUT " +
        "replacement by default (randsamp(v, k)) — the same default TI calculators use " +
        "— so no element is drawn more often than it appears in v, and k cannot exceed " +
        "v's length. Pass a third argument, randsamp(v, k, withReplacement), where " +
        "withReplacement is any nonzero number, to sample WITH replacement instead: each " +
        "draw is independent, k may exceed v's length, and the same element can be drawn " +
        "more than once even from a vector with no repeated values. k must be a positive " +
        "integer either way.";

    public IReadOnlyList<string> Examples => _hasReplacementArgument
        ? new[] { "randsamp([1,2,3], 5, 1) → a 1x5 vector, duplicates possible, e.g. [2,2,1,3,2]" }
        : new[]
        {
            "randsamp([1,2,3,4,5], 3) → 3 distinct elements of the vector, e.g. [4,1,5]",
            "randsamp([1,2,3], 5) → Error: randsamp without replacement requires k <= the vector's length (3), got k=5.",
        };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var vector = FunctionArguments.RequireVector(arguments[0], "randsamp");
        var k = FunctionArguments.RequireInteger(arguments[1], "randsamp");
        if (k <= 0)
            throw new InvalidOperationException("randsamp requires a positive integer k.");

        var withReplacement = _hasReplacementArgument
            && FunctionArguments.RequireNumber(arguments[2], "randsamp") != 0;

        if (withReplacement)
        {
            var sample = new double[k];
            for (var i = 0; i < k; i++)
                sample[i] = vector[_source.NextInt(0, vector.Length - 1)];
            return MatrixValue.FromRow(sample);
        }

        if (k > vector.Length)
            throw new InvalidOperationException(
                $"randsamp without replacement requires k <= the vector's length ({vector.Length}), got k={k}.");

        // Partial Fisher-Yates: only shuffles the first k positions, since that's all
        // that's needed -- no need to permute the whole vector to sample from it.
        var pool = (double[])vector.Clone();
        for (var i = 0; i < k; i++)
        {
            var j = _source.NextInt(i, pool.Length - 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return MatrixValue.FromRow(pool[..k]);
    }
}
