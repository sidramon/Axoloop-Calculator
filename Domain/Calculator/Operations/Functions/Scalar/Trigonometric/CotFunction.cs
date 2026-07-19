namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class CotFunction : IFunction
{
    public string Name => "cot";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "cot(x)";

    public string Description =>
        "Cotangent = cos(x) / sin(x). x is in RADIANS. Undefined at multiples of π; " +
        "throws, unlike tan (its reciprocal) which does not detect its own poles.";

    public IReadOnlyList<string> Examples => new[]
    {
        "cot(_pi / 4) → 1",
        "cot(_pi) → Error: cot is undefined at multiples of pi.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("cot requires a number.");

        var sin = Math.Sin(n.Number);
        if (Math.Abs(sin) < 1e-12)
            throw new InvalidOperationException("cot is undefined at multiples of pi.");

        return new NumberValue(Math.Cos(n.Number) / sin);
    }
}
