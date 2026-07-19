namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class Atan2Function : IFunction
{
    public string Name => "atan2";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "atan2(y, x)";

    public string Description =>
        "Two-argument arc tangent; the result is in RADIANS and covers all four quadrants " +
        "(unlike atan). Argument order: atan2(y, x) — the y coordinate first, then x — " +
        "standard mathematical convention but easy to swap by mistake.";

    public IReadOnlyList<string> Examples => new[]
    {
        "atan2(1, 1) → 0.7853981634",
        "atan2(1, -1) → 2.3561944902",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue y || arguments[1] is not NumberValue x)
            throw new InvalidOperationException("atan2 requires two numbers.");
        return new NumberValue(Math.Atan2(y.Number, x.Number));
    }
}
