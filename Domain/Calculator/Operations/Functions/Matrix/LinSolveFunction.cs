namespace Domain.Calculator.Operations.Functions.Matrix;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;

public sealed class LinSolveFunction : IFunction
{
    public string Name => "linsolve";
    public int Arity => 2;
    public FunctionCategory Category => FunctionCategory.Matrix;
    public string Signature => "linsolve(a, b)";

    public string Description =>
        "Solves the linear system a*x = b, given in matrix form, by Gauss-Jordan elimination " +
        "with partial pivoting. b is accepted as either a column (n x 1) or row (1 x n) " +
        "vector; the result is always a column vector. Rectangular systems (more equations " +
        "than unknowns, or fewer) are supported. Throws if the system has no solution, or if " +
        "it has infinitely many — use linsolvegen to get the general solution in that case. " +
        "For an equation written literally, such as '3*x + 2 = 5', use solve instead.";

    public IReadOnlyList<string> Examples => new[]
    {
        "linsolve([1,1;1,-1], [3,1]) → [2;1]",
        "linsolve([1,1;1,1], [1,2]) → Error: system has no solution (rank(a)=1, rank([a|b])=2).",
    };

    public Value Apply(IReadOnlyList<Value> arguments)
    {
        var a = FunctionArguments.RequireMatrix(arguments[0], "linsolve");
        var b = FunctionArguments.RequireMatrix(arguments[1], "linsolve");

        var solution = LinearSolver.Solve(a, b);

        return solution.Kind switch
        {
            SolutionKind.Unique => solution.Particular!,
            SolutionKind.None => throw new InvalidOperationException(
                $"system has no solution (rank(a)={solution.Rank}, rank([a|b])={solution.Rank + 1})."),
            SolutionKind.Infinite => throw new InvalidOperationException(
                $"system has infinitely many solutions ({solution.FreeVariables} free variable(s)); " +
                "use linsolvegen to get the general solution."),
            _ => throw new InvalidOperationException("Unexpected solution kind."),
        };
    }
}
