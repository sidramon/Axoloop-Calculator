namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class SumFunction : IFunction
{
    public string Name => "sum";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "sum(v)";

    public string Description => "Sum of a vector's elements (1xN or Nx1 matrix). Throws on an empty vector.";

    public IReadOnlyList<string> Examples => new[]
    {
        "sum([1,2,3,4]) → 10",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(DescriptiveStatistics.Sum(FunctionArguments.RequireVector(arguments[0], "sum")));
}
