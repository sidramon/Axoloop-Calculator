namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class CscFunction : IFunction
{
    public string Name => "csc";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "csc(x)";

    public string Description =>
        "Cosecant = 1 / sin(x). x is in RADIANS. Undefined at multiples of π (where " +
        "sin(x) = 0); throws instead of returning an infinite value.";

    public IReadOnlyList<string> Examples => new[]
    {
        "csc(_pi / 2) → 1",
        "csc(_pi) → Error: csc is undefined at multiples of pi.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("csc requires a number.");

        var sin = Math.Sin(n.Number);
        if (Math.Abs(sin) < 1e-12)
            throw new InvalidOperationException("csc is undefined at multiples of pi.");

        return new NumberValue(1 / sin);
    }
}
