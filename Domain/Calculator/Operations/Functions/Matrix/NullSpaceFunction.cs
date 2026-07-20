namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class NullSpaceFunction : IFunction
{
    public string Name => "nullspace";
    public int Arity => 1;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "nullspace(m)";

    public string Description =>
        "Basis of the null space of m (the solutions of m*x = 0), one column per basis " +
        "vector. A trivial null space (only the zero vector, i.e. m has full column rank) " +
        "throws rather than returning an empty or all-zero matrix, since there is no basis " +
        "vector to display.";

    public IReadOnlyList<string> Examples => new[]
    {
        "nullspace([1,1;1,1]) → [-1;1]",
        "nullspace([1,0;0,1]) → Error: nullspace is trivial (m has full column rank).",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var m = FunctionArguments.RequireMatrix(arguments[0], "nullspace");
        var zero = MatrixValue.Filled(m.Rows, 1, 0);
        var solution = LinearSolver.Solve(m, zero);

        if (solution.NullSpaceBasis.Count == 0)
            throw new InvalidOperationException("nullspace is trivial (m has full column rank).");

        var rows = m.Columns;
        var columns = solution.NullSpaceBasis.Count;
        var data = new double[rows, columns];
        for (var col = 0; col < columns; col++)
            for (var r = 0; r < rows; r++)
                data[r, col] = solution.NullSpaceBasis[col][r, 0];

        return new MatrixValue(data);
    }
}
