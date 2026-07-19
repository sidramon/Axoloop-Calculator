namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class AsinFunction : IFunction
{
    public string Name => "asin";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "asin(x)";

    public string Description =>
        "Arc sine; the result is in RADIANS. Requires -1 <= x <= 1 (the range of sin); " +
        "outside that interval, throws instead of returning NaN.";

    public IReadOnlyList<string> Examples => new[]
    {
        "asin(1) → 1.5707963268",
        "asin(2) → Error: asin requires a number between -1 and 1.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("asin requires a number.");
        if (n.Number < -1 || n.Number > 1)
            throw new InvalidOperationException("asin requires a number between -1 and 1.");
        return new NumberValue(Math.Asin(n.Number));
    }
}
