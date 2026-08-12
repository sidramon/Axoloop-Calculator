namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class MeanFunction : IFunction
{
    public string Name => "mean";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "mean(v)";

    public string Description =>
        "Arithmetic mean of a vector (1xN or Nx1 matrix). Throws on an empty vector.";

    public IReadOnlyList<string> Examples => new[]
    {
        "mean([1,2,3,4]) → 2.5",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(DescriptiveStatistics.Mean(FunctionArguments.RequireVector(arguments[0], "mean")));
}
