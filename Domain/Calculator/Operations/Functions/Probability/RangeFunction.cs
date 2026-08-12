namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class RangeFunction : IFunction
{
    public string Name => "range";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "range(v)";

    public string Description => "Range of a vector (1xN or Nx1 matrix): max(v) - min(v). Throws on an empty vector.";

    public IReadOnlyList<string> Examples => new[]
    {
        "range([3,1,4,1,5]) → 4",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(DescriptiveStatistics.Range(FunctionArguments.RequireVector(arguments[0], "range")));
}
