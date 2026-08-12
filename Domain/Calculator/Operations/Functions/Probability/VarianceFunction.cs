namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

/// <summary>
/// variance(v) and variance(v, population) — registered twice under the name "variance",
/// at arity 1 (sample variance, the default) and arity 2 (explicit population flag),
/// mirroring how AntiderivativeFunction covers two arities of "integral". The language
/// has no boolean literal, so the flag is a plain number: 0 (or omitted entirely, arity 1)
/// selects sample variance, any nonzero value selects population variance.
/// </summary>
public sealed class VarianceFunction : IFunction
{
    private readonly bool _hasPopulationArgument;

    public VarianceFunction(bool hasPopulationArgument = false) => _hasPopulationArgument = hasPopulationArgument;

    public string Name => "variance";
    public int Arity => _hasPopulationArgument ? 2 : 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => _hasPopulationArgument ? "variance(v, population)" : "variance(v)";

    public string Description =>
        "Variance of a vector (1xN or Nx1 matrix). variance(v) computes the SAMPLE " +
        "variance (Bessel's correction: divides the sum of squared deviations by n-1) — " +
        "the standard choice when v is a sample used to estimate a wider population's " +
        "variance, and what Excel's VAR.S, R's var(), and most calculators default to. " +
        "Pass a second argument, variance(v, population), where population is any nonzero " +
        "number, to get the POPULATION variance instead (divides by n) — only correct when " +
        "v IS the entire population rather than a sample drawn from it. The two differ by " +
        "a factor of n/(n-1), a common source of \"why doesn't this match Excel\" confusion " +
        "if the convention goes unstated. Sample variance requires at least 2 values (n-1 " +
        "would be 0 otherwise); population variance accepts a single value (variance 0). " +
        "Throws on an empty vector.";

    public IReadOnlyList<string> Examples => _hasPopulationArgument
        ? new[]
        {
            "variance([2,4,4,4,5,5,7,9], 1) → 4  (population variance, divides by n=8)",
        }
        : new[]
        {
            "variance([2,4,4,4,5,5,7,9]) → 4.5714285714  (sample variance, divides by n-1=7)",
        };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var vector = FunctionArguments.RequireVector(arguments[0], "variance");
        var population = _hasPopulationArgument && FunctionArguments.RequireNumber(arguments[1], "variance") != 0;
        return new NumberValue(DescriptiveStatistics.Variance(vector, population));
    }
}
