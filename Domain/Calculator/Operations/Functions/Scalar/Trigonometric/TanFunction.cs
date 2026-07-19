namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class TanFunction : IFunction
{
    public string Name => "tan";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "tan(x)";

    public string Description =>
        "Tangent. x is in RADIANS, not degrees. Unlike cot (its reciprocal), tan does not " +
        "detect its own poles: tan(_pi / 2) does not throw, it returns a very large " +
        "numeric value (a floating-point rounding artifact) instead of an error.";

    public IReadOnlyList<string> Examples => new[]
    {
        "tan(0) → 0",
        "tan(_pi / 4) → 1",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("tan requires a number.");
        return new NumberValue(Math.Tan(n.Number));
    }
}
