namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

/// <summary>std(v) and std(v, population) — same arity-overload and flag convention as <see cref="VarianceFunction"/>.</summary>
public sealed class StdDevFunction : IFunction
{
    private readonly bool _hasPopulationArgument;

    public StdDevFunction(bool hasPopulationArgument = false) => _hasPopulationArgument = hasPopulationArgument;

    public string Name => "std";
    public int Arity => _hasPopulationArgument ? 2 : 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => _hasPopulationArgument ? "std(v, population)" : "std(v)";

    public string Description =>
        "Standard deviation of a vector (1xN or Nx1 matrix) — the square root of variance, " +
        "with the exact same sample-vs-population convention as variance: std(v) is the " +
        "SAMPLE standard deviation (n-1 denominator) by default; std(v, population), with " +
        "population any nonzero number, gives the POPULATION standard deviation (n " +
        "denominator) instead. See variance's documentation for why the distinction " +
        "matters. Throws on an empty vector.";

    public IReadOnlyList<string> Examples => _hasPopulationArgument
        ? new[]
        {
            "std([2,4,4,4,5,5,7,9], 1) → 2  (population standard deviation)",
        }
        : new[]
        {
            "std([2,4,4,4,5,5,7,9]) → 2.1380899353  (sample standard deviation)",
        };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var vector = FunctionArguments.RequireVector(arguments[0], "std");
        var population = _hasPopulationArgument && FunctionArguments.RequireNumber(arguments[1], "std") != 0;
        return new NumberValue(DescriptiveStatistics.StandardDeviation(vector, population));
    }
}
