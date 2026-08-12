namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Values;

public sealed class AbsFunction : IFunction
{
    public string Name => "abs";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "abs(x)";

    public string Description =>
        "Absolute value of a real number, or the modulus |a+bi| = sqrt(a^2+b^2) of a " +
        "complex one — the same notion of \"distance from zero\", specialized per type. " +
        "Works on scalars only: there is no element-wise variant for matrices.";

    public IReadOnlyList<string> Examples => new[]
    {
        "abs(-5) → 5",
        "abs(5) → 5",
        "abs(3 + 4*_i) → 5",
    };

    public Value Apply(IReadOnlyList<Value> arguments) => arguments[0] switch
    {
        NumberValue n => new NumberValue(Math.Abs(n.Number)),
        ComplexValue c => new NumberValue(c.Modulus()),
        _ => throw new InvalidOperationException("abs requires a number.")
    };
}
