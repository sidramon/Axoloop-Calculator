namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class AcosFunction : IFunction
{
    public string Name => "acos";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "acos(x)";

    public string Description =>
        "Arc cosine; the result is in RADIANS. Requires -1 <= x <= 1 (the range of cos); " +
        "outside that interval, throws instead of returning NaN.";

    public IReadOnlyList<string> Examples => new[]
    {
        "acos(1) → 0",
        "acos(-2) → Error: acos requires a number between -1 and 1.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("acos requires a number.");
        if (n.Number < -1 || n.Number > 1)
            throw new InvalidOperationException("acos requires a number between -1 and 1.");
        return new NumberValue(Math.Acos(n.Number));
    }
}
