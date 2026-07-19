namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Values;

public sealed class SqrtFunction : IFunction
{
    public string Name => "sqrt";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "sqrt(x)";

    public string Description =>
        "Square root. Requires x >= 0: negative numbers throw instead of producing a " +
        "complex result. For an odd root of a negative number, use nthroot.";

    public IReadOnlyList<string> Examples => new[]
    {
        "sqrt(9) → 3",
        "sqrt(-1) → Error: sqrt requires a non-negative number.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("sqrt requires a number.");
        if (n.Number < 0)
            throw new InvalidOperationException("sqrt requires a non-negative number.");
        return new NumberValue(Math.Sqrt(n.Number));
    }
}
