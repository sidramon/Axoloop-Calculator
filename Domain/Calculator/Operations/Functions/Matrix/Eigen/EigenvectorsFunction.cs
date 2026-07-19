using Domain.Calculator.Algorithms;

namespace Domain.Calculator.Operations.Functions.Matrix.Eigen;

using Domain.Calculator.Values;

public sealed class EigenvectorsFunction : IFunction
{
    public string Name => "eigvecs";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Eigen;
    public string Signature => "eigvecs(m)";

    public string Description =>
        "Eigenvectors (one column per vector), aligned with the descending eigenvalue " +
        "order from eigvals. Only supports SYMMETRIC matrices for now; throws otherwise, " +
        "unlike eigvals which also accepts non-symmetric real matrices.";

    public IReadOnlyList<string> Examples => new[]
    {
        "eigvecs([2,0;0,3]) → [0,1;1,0] (columns aligned with eigvals([2,0;0,3]) = [3,2])",
        "eigvecs([0,1;-1,0]) → Error: eigvecs currently supports symmetric matrices only.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue m)
            throw new InvalidOperationException("eigvecs requires a matrix.");

        if (!EigenDecomposition.IsSymmetric(m))
            throw new InvalidOperationException(
                "eigvecs currently supports symmetric matrices only.");

        var (_, vectors) = EigenDecomposition.Symmetric(m);
        return new MatrixValue(vectors);
    }
}
