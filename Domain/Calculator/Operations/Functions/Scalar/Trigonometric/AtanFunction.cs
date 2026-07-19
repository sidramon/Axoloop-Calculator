namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class AtanFunction : IFunction
{
    public string Name => "atan";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "atan(x)";

    public string Description =>
        "Single-argument arc tangent; the result is in RADIANS, in the range (-π/2, π/2). " +
        "Unlike atan2, it only knows the ratio y/x and so cannot distinguish opposite " +
        "quadrants; prefer atan2(y, x) when the two separate components are known.";

    public IReadOnlyList<string> Examples => new[]
    {
        "atan(1) → 0.7853981634",
        "atan(0) → 0",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("atan requires a number.");
        return new NumberValue(Math.Atan(n.Number));
    }
}
