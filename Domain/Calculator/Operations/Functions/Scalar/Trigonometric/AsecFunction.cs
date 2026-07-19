namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class AsecFunction : IFunction
{
    public string Name => "asec";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "asec(x)";

    public string Description =>
        "Arc secant; the result is in RADIANS. Requires |x| >= 1, since the range of sec " +
        "excludes the open interval (-1, 1). Computed as acos(1 / x).";

    public IReadOnlyList<string> Examples => new[]
    {
        "asec(1) → 0",
        "asec(0.5) → Error: asec requires |x| >= 1.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("asec requires a number.");
        if (Math.Abs(n.Number) < 1)
            throw new InvalidOperationException("asec requires |x| >= 1.");

        return new NumberValue(Math.Acos(1 / n.Number));
    }
}
