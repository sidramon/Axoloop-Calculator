using Domain.Calculator.Algorithms;

namespace Domain.Calculator.Operations.Functions.Matrix.Eigen;

using Domain.Calculator.Values;

public sealed class EigenvaluesFunction : IFunction
{
    public string Name => "eigvals";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Eigen;
    public string Signature => "eigvals(m)";

    public string Description =>
        "Eigenvalues, sorted in DESCENDING order. Uses the Jacobi method if m is symmetric, " +
        "otherwise the QR algorithm with Wilkinson shift (iterative, may not converge on " +
        "pathological cases). Throws if complex eigenvalues are detected: only real " +
        "eigenvalues are supported. Requires a square matrix.";

    public IReadOnlyList<string> Examples => new[]
    {
        "eigvals([2,0;0,3]) → [3,2]",
        "eigvals([0,-1;1,0]) → Error: Matrix has complex eigenvalues; only real eigenvalues are supported.",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue m)
            throw new InvalidOperationException("eigvals requires a matrix.");

        var values = EigenDecomposition.General(m);

        var data = new double[1, values.Length];
        for (var i = 0; i < values.Length; i++) data[0, i] = values[i];
        return new MatrixValue(data);
    }
}
