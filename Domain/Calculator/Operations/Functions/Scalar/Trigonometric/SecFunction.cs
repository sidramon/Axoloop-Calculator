namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class SecFunction : IFunction
{
    public string Name => "sec";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "sec(x)";

    public string Description =>
        "Secant = 1 / cos(x). x is in RADIANS. Undefined at odd multiples of π/2 (where " +
        "cos(x) = 0); throws instead of returning an infinite value.";

    public IReadOnlyList<string> Examples => new[]
    {
        "sec(0) → 1",
        "sec(_pi / 2) → Error: sec is undefined at odd multiples of pi/2.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("sec requires a number.");

        var cos = Math.Cos(n.Number);
        if (Math.Abs(cos) < 1e-12)
            throw new InvalidOperationException("sec is undefined at odd multiples of pi/2.");

        return new NumberValue(1 / cos);
    }
}
