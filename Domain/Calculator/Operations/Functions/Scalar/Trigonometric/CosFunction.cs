namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class CosFunction : IFunction
{
    public string Name => "cos";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "cos(x)";

    public string Description =>
        "Cosine. x is in RADIANS, not degrees. To convert from degrees: " +
        "cos(degrees * _pi / 180).";

    public IReadOnlyList<string> Examples => new[]
    {
        "cos(0) → 1",
        "cos(_pi) → -1",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("cos requires a number.");
        return new NumberValue(Math.Cos(n.Number));
    }
}
