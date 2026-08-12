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
        "pathological cases). A 2x2 matrix with complex eigenvalues now returns its complex " +
        "conjugate pair instead of throwing; a LARGER matrix with complex eigenvalues still " +
        "throws — extracting those from the QR iteration is not yet supported. Requires a " +
        "square matrix.";

    public IReadOnlyList<string> Examples => new[]
    {
        "eigvals([2,0;0,3]) → [3,2]",
        "eigvals([0,-1;1,0]) → [i, -i]",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        if (arguments[0] is not MatrixValue m)
            throw new InvalidOperationException("eigvals requires a matrix.");

        if (EigenDecomposition.TwoByTwoComplexPair(m) is { } pair)
            return new ValueListValue(new[] { ComplexValue.Of(pair.First), ComplexValue.Of(pair.Second) });

        var values = EigenDecomposition.General(m);

        var data = new double[1, values.Length];
        for (var i = 0; i < values.Length; i++) data[0, i] = values[i];
        return new MatrixValue(data);
    }
}
