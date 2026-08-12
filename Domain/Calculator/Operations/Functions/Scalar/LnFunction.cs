namespace Domain.Calculator.Operations.Functions.Scalar;

using System.Numerics;
using Domain.Calculator.Values;

public sealed class LnFunction : IFunction
{
    public string Name => "ln";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "ln(x)";

    public string Description =>
        "Natural logarithm (base e). x = 0 still throws — there is no complex value to " +
        "fall back to there, unlike IEEE 754's -Infinity. A negative x now returns a " +
        "complex result (ln(-1) → πi) instead of throwing.";

    public IReadOnlyList<string> Examples => new[]
    {
        "ln(_e) → 1",
        "ln(-1) → 3.1416i",
        "ln(0) → Error: ln requires a nonzero number.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("ln requires a number.");
        if (n.Number == 0)
            throw new InvalidOperationException("ln requires a nonzero number.");
        if (n.Number < 0)
            return ComplexValue.Of(Complex.Log(new Complex(n.Number, 0)));
        return new NumberValue(Math.Log(n.Number));
    }
}
