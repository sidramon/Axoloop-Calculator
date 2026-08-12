namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class MedianFunction : IFunction
{
    public string Name => "median";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "median(v)";

    public string Description =>
        "Median of a vector (1xN or Nx1 matrix): the middle element once sorted, or the " +
        "average of the two middle elements when the vector has even length. Throws on " +
        "an empty vector.";

    public IReadOnlyList<string> Examples => new[]
    {
        "median([1,3,2]) → 2",
        "median([1,2,3,4]) → 2.5",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(DescriptiveStatistics.Median(FunctionArguments.RequireVector(arguments[0], "median")));
}
