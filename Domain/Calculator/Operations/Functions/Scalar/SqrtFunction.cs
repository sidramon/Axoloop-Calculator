namespace Domain.Calculator.Operations.Functions.Scalar;

using System.Numerics;
using Domain.Calculator.Values;

public sealed class SqrtFunction : IFunction
{
    public string Name => "sqrt";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "sqrt(x)";

    public string Description =>
        "Square root. x >= 0 returns a real result as before; a negative x now returns a " +
        "complex result (sqrt(-1) → i) instead of throwing. For an odd root of a negative " +
        "number, use nthroot.";

    public IReadOnlyList<string> Examples => new[]
    {
        "sqrt(9) → 3",
        "sqrt(-1) → i",
        "sqrt(-4) → 2i",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("sqrt requires a number.");
        if (n.Number < 0)
            return ComplexValue.Of(Complex.Sqrt(new Complex(n.Number, 0)));
        return new NumberValue(Math.Sqrt(n.Number));
    }
}
