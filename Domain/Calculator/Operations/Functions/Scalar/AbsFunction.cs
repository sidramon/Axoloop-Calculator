namespace Domain.Calculator.Operations.Functions.Scalar;

using Domain.Calculator.Values;

public sealed class AbsFunction : IFunction
{
    public string Name => "abs";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Arithmetic;
    public string Signature => "abs(x)";

    public string Description =>
        "Absolute value of a number. Works on scalars only: there is no element-wise " +
        "variant for matrices.";

    public IReadOnlyList<string> Examples => new[]
    {
        "abs(-5) → 5",
        "abs(5) → 5",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("abs requires a number.");
        return new NumberValue(Math.Abs(n.Number));
    }
}
