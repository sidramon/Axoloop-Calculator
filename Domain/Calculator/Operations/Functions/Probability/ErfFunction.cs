namespace Domain.Calculator.Operations.Functions.Probability;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class ErfFunction : IFunction
{
    public string Name => "erf";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Statistics;
    public string Signature => "erf(x)";

    public string Description =>
        "Error function: erf(x) = (2/sqrt(pi)) * the integral of exp(-t^2) from 0 to x. " +
        "The building block behind the normal distribution's CDF (normcdf). Computed via " +
        "the Abramowitz & Stegun 7.1.26 rational approximation — accurate to about 1e-7, " +
        "not the ~1e-15 of a full-precision implementation, but far more than the normal " +
        "distribution functions built on it need.";

    public IReadOnlyList<string> Examples => new[]
    {
        "erf(0) → 0",
        "erf(1) → 0.8427008",
    };

    public Value Apply(IReadOnlyList<Value> arguments) =>
        new NumberValue(SpecialFunctions.Erf(FunctionArguments.RequireNumber(arguments[0], "erf")));
}
