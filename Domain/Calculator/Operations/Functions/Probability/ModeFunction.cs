namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class ModeFunction : IFunction
{
    public string Name => "mode";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "mode(v)";

    public string Description =>
        "Most frequent value in a vector (1xN or Nx1 matrix). A multimodal vector — several " +
        "values tied for most frequent — has no single correct mode; ties are broken by " +
        "returning the SMALLEST of the tied values, an arbitrary but deterministic choice. " +
        "Throws on an empty vector.";

    public IReadOnlyList<string> Examples => new[]
    {
        "mode([1,2,2,3]) → 2",
        "mode([1,1,2,2]) → 1  (tie broken toward the smaller value)",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(DescriptiveStatistics.Mode(FunctionArguments.RequireVector(arguments[0], "mode")));
}
