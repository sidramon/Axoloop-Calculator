namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Values;

public sealed class NormFunction : IFunction
{
    public string Name => "norm";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "norm(v)";

    public string Description =>
        "Euclidean (L2) norm of a vector (1×N or N×1 matrix) — the square root of the " +
        "sum of its squared elements, i.e. its length. Throws if the argument is not a vector.";

    public IReadOnlyList<string> Examples => new[]
    {
        "norm([3,4]) → 5",
        "norm([1;1;1;1]) → 2",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue v)
            throw new InvalidOperationException("norm requires a vector.");
        return new NumberValue(v.Norm());
    }
}
