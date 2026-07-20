namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class DeterminantFunction : IFunction
{
    public string Name => "det";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "det(m)";

    public string Description =>
        "Determinant, computed by LU decomposition with partial pivoting. O(n^3) cost, so " +
        "it scales well past a dozen rows, unlike a cofactor-expansion implementation. " +
        "Requires a square matrix.";

    public IReadOnlyList<string> Examples => new[]
    {
        "det([1,2;3,4]) → -2",
        "det([1,2,3;4,5,6]) → Error: Determinant requires a square matrix.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue m)
            throw new InvalidOperationException("det requires a matrix.");
        return new NumberValue(Determinant.Compute(m));
    }
}
