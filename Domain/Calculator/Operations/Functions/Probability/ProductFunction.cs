namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class ProductFunction : IFunction
{
    public string Name => "product";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "product(v)";

    public string Description => "Product of a vector's elements (1xN or Nx1 matrix). Throws on an empty vector.";

    public IReadOnlyList<string> Examples => new[]
    {
        "product([1,2,3,4]) → 24",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(DescriptiveStatistics.Product(FunctionArguments.RequireVector(arguments[0], "product")));
}
