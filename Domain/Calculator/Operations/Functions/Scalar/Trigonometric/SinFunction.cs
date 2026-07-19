namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class SinFunction : IFunction
{
    public string Name => "sin";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "sin(x)";

    public string Description =>
        "Sine. x is in RADIANS, not degrees — the most common mistake. To convert from " +
        "degrees: sin(degrees * _pi / 180).";

    public IReadOnlyList<string> Examples => new[]
    {
        "sin(0) → 0",
        "sin(_pi / 2) → 1",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("sin requires a number.");
        return new NumberValue(Math.Sin(n.Number));
    }
}
