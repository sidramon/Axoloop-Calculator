namespace Domain.Calculator.Operations.Functions.Random;

using Domain.Calculator.Random;
using Domain.Calculator.Values;

/// <summary>
/// seed(k) — the one function in this calculator whose entire purpose is a side effect.
/// Every other IFunction.Apply is a pure computation on its arguments; this one mutates
/// the shared IRandomSource instead, so every random draw afterward (rand, randint,
/// randnorm, randbin, randsamp) becomes reproducible from that point on. It's still an
/// ordinary function call, not a special form: nothing about evaluating its argument or
/// dispatching the call is special, only what Apply does with the injected source.
/// It still returns a value, for consistency with the rest of the language (every call
/// is an expression with a result) -- specifically k itself, echoed back, so "seed(42)"
/// reads at the REPL as confirmation that the seed took effect, without inventing a new
/// kind of "nothing useful happened" return value.
/// </summary>
public sealed class SeedFunction : IFunction
{
    private readonly IRandomSource _source;

    public SeedFunction(IRandomSource source) => _source = source;

    public string Name => "seed";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Random;
    public string Signature => "seed(k)";

    public string Description =>
        "Reseeds the shared random source with k, making every random function called " +
        "afterward (rand, randint, randnorm, randbin, randsamp) produce a reproducible " +
        "sequence from that point on -- the same seed always starts the same sequence. " +
        "This is a side effect on shared state, unlike every other function here; it " +
        "still returns a value for consistency with the rest of the language, echoing " +
        "back k itself.";

    public IReadOnlyList<string> Examples => new[]
    {
        "seed(42) → 42",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var seed = FunctionArguments.RequireInteger(arguments[0], "seed");
        _source.Seed(seed);
        return new NumberValue(seed);
    }
}
