namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class ErfcFunction : IFunction
{
    public string Name => "erfc";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "erfc(x)";

    public string Description =>
        "Complementary error function: erfc(x) = 1 - erf(x). Same Abramowitz & Stegun " +
        "approximation and ~1e-7 accuracy as erf.";

    public IReadOnlyList<string> Examples => new[]
    {
        "erfc(0) → 1",
        "erfc(1) → 0.1572992",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(SpecialFunctions.Erfc(FunctionArguments.RequireNumber(arguments[0], "erfc")));
}
