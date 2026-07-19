namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class AcotFunction : IFunction
{
    public string Name => "acot";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "acot(x)";

    public string Description =>
        "Arc cotangent; the result is in RADIANS. Defined for every real number — unlike " +
        "acsc/asec, it has no domain restriction. Computed as π/2 - atan(x).";

    public IReadOnlyList<string> Examples => new[]
    {
        "acot(0) → 1.5707963268",
        "acot(1) → 0.7853981634",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("acot requires a number.");

        return new NumberValue(Math.PI / 2 - Math.Atan(n.Number));
    }
}
