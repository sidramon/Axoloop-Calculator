namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class LinSolveGeneralFunction : IFunction
{
    public string Name => "linsolvegen";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "linsolvegen(a, b)";

    public string Description =>
        "Solves the linear system a*x = b, given in matrix form, and always returns a " +
        "matrix, even when there are infinitely many solutions. A unique solution is " +
        "returned as a single column, exactly like linsolve. When there are infinitely many " +
        "solutions, the first column is a particular solution (free variables set to zero) " +
        "and each remaining column is a basis vector of the null space of a, one per free " +
        "variable: the general solution is particular + t1*column2 + t2*column3 + ... for " +
        "any real t1, t2, .... b is accepted as either a column (n x 1) or row (1 x n) " +
        "vector. Throws if the system has no solution.";

    public IReadOnlyList<string> Examples => new[]
    {
        "linsolvegen([1,1;1,-1], [3,1]) → [2;1] (unique, a single column)",
        "linsolvegen([1,1], [2]) → [2,-1;0,1] (particular [2;0], null-space basis [-1;1])",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var a = FunctionArguments.RequireMatrix(arguments[0], "linsolvegen");
        var b = FunctionArguments.RequireMatrix(arguments[1], "linsolvegen");

        var solution = LinearSolver.Solve(a, b);

        if (solution.Kind == SolutionKind.None)
            throw new InvalidOperationException(
                $"system has no solution (rank(a)={solution.Rank}, rank([a|b])={solution.Rank + 1}).");

        if (solution.NullSpaceBasis.Count == 0)
            return solution.Particular!;

        var rows = solution.Particular!.Rows;
        var columns = 1 + solution.NullSpaceBasis.Count;
        var data = new double[rows, columns];
        for (var r = 0; r < rows; r++) data[r, 0] = solution.Particular[r, 0];
        for (var col = 0; col < solution.NullSpaceBasis.Count; col++)
            for (var r = 0; r < rows; r++)
                data[r, col + 1] = solution.NullSpaceBasis[col][r, 0];

        return new MatrixValue(data);
    }
}
