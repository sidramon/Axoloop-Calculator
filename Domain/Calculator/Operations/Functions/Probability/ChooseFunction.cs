namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class ChooseFunction : IFunction
{
    public string Name => "choose";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "choose(n, k)";

    public string Description =>
        "Binomial coefficient C(n,k) — the number of ways to choose k items from n " +
        "without regard to order. Computed via the multiplicative formula rather than " +
        "n!/(k!(n-k)!), so it never forms a factorial directly and doesn't overflow the " +
        "way 100! would; precision instead degrades gradually as n grows, staying exact " +
        "(rounds to the precise integer) through roughly n = 50, and no longer landing " +
        "on the exact integer from around n = 60 on — though still correct to about 15-16 " +
        "significant figures at any n. Both n and k must be non-negative integers; k > n " +
        "returns 0 rather than throwing.";

    public IReadOnlyList<string> Examples => new[]
    {
        "choose(5, 2) → 10",
        "choose(52, 5) → 2598960",
        "choose(5, 10) → 0",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var n = FunctionArguments.RequireInteger(arguments[0], "choose");
        var k = FunctionArguments.RequireInteger(arguments[1], "choose");
        return new NumberValue(Combinatorics.Choose(n, k));
    }
}
