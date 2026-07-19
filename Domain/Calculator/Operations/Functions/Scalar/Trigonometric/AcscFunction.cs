namespace Domain.Calculator.Operations.Functions.Scalar.Trigonometric;

using Domain.Calculator.Values;

public sealed class AcscFunction : IFunction
{
    public string Name => "acsc";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Trigonometry;
    public string Signature => "acsc(x)";

    public string Description =>
        "Arc cosecant; the result is in RADIANS. Requires |x| >= 1, since the range of csc " +
        "excludes the open interval (-1, 1). Computed as asin(1 / x).";

    public IReadOnlyList<string> Examples => new[]
    {
        "acsc(1) → 1.5707963268",
        "acsc(0.5) → Error: acsc requires |x| >= 1.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("acsc requires a number.");
        if (Math.Abs(n.Number) < 1)
            throw new InvalidOperationException("acsc requires |x| >= 1.");

        return new NumberValue(Math.Asin(1 / n.Number));
    }
}
