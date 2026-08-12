namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class PermutationsFunction : IFunction
{
    public string Name => "perm";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "perm(n, k)";

    public string Description =>
        "Number of permutations P(n,k) = n!/(n-k)! — the count of ordered arrangements of " +
        "k items drawn from n. Like choose, computed as a running product of k terms " +
        "rather than via two factorials, avoiding the overflow a direct n!/(n-k)! would " +
        "hit for large n. Both n and k must be non-negative integers; k > n returns 0 " +
        "rather than throwing.";

    public IReadOnlyList<string> Examples => new[]
    {
        "perm(5, 2) → 20",
        "perm(10, 3) → 720",
        "perm(5, 10) → 0",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var n = FunctionArguments.RequireInteger(arguments[0], "perm");
        var k = FunctionArguments.RequireInteger(arguments[1], "perm");
        return new NumberValue(Combinatorics.Permutations(n, k));
    }
}
