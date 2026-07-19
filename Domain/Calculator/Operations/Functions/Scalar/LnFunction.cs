namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Values;

public sealed class LnFunction : IFunction
{
    public string Name => "ln";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "ln(x)";

    public string Description =>
        "Natural logarithm (base e). Requires x > 0: ln(0) and negative x throw, unlike " +
        "IEEE 754 which would return -Infinity and NaN respectively.";

    public IReadOnlyList<string> Examples => new[]
    {
        "ln(_e) → 1",
        "ln(0) → Error: ln requires a positive number.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("ln requires a number.");
        if (n.Number <= 0)
            throw new InvalidOperationException("ln requires a positive number.");
        return new NumberValue(Math.Log(n.Number));
    }
}
