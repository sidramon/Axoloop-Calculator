namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Values;

public sealed class IdentityFunction : IFunction
{
    public string Name => "identity";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "identity(n)";

    public string Description =>
        "n×n identity matrix (ones on the diagonal, zeros elsewhere). n must be an integer " +
        "greater than or equal to 1.";

    public IReadOnlyList<string> Examples => new[]
    {
        "identity(2) → [1,0;0,1]",
        "identity(2.5) → Error: identity size must be an integer.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not NumberValue n)
            throw new InvalidOperationException("identity requires a number (the size).");
        if (n.Number % 1 != 0)
            throw new InvalidOperationException("identity size must be an integer.");
        return MatrixValue.Identity((int)n.Number);
    }
}
