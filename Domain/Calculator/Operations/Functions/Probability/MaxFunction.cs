namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class MaxFunction : IFunction
{
    public string Name => "max";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "max(v)";

    public string Description => "Largest element of a vector (1xN or Nx1 matrix). Throws on an empty vector.";

    public IReadOnlyList<string> Examples => new[]
    {
        "max([3,1,4,1,5]) → 5",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(DescriptiveStatistics.Max(FunctionArguments.RequireVector(arguments[0], "max")));
}
