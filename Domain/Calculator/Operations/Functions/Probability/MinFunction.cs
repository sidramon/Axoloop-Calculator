namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class MinFunction : IFunction
{
    public string Name => "min";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "min(v)";

    public string Description => "Smallest element of a vector (1xN or Nx1 matrix). Throws on an empty vector.";

    public IReadOnlyList<string> Examples => new[]
    {
        "min([3,1,4,1,5]) → 1",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(DescriptiveStatistics.Min(FunctionArguments.RequireVector(arguments[0], "min")));
}
