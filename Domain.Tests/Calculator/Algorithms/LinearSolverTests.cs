namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;
using FluentAssertions;

public class LinearSolverTests
{
    private static MatrixValue M(double[,] data) => new(data);

    private static void AssertSatisfies(MatrixValue a, MatrixValue x, MatrixValue b)
    {
        var actual = a.Multiply(x);
        for (var r = 0; r < b.Rows; r++)
            actual[r, 0].Should().BeApproximately(b[r, 0], 1e-9);
    }

    [Fact]
    public void Solve_UniqueSolution2x2_SatisfiesTheSystemBySubstitution()
    {
        var a = M(new double[,] { { 1, 1 }, { 1, -1 } });
        var expectedX = M(new double[,] { { 2 }, { 1 } });
        var b = a.Multiply(expectedX);

        var solution = LinearSolver.Solve(a, b);

        solution.Kind.Should().Be(SolutionKind.Unique);
        solution.FreeVariables.Should().Be(0);
        AssertSatisfies(a, solution.Particular!, b);
        solution.Particular![0, 0].Should().BeApproximately(2, 1e-9);
        solution.Particular![1, 0].Should().BeApproximately(1, 1e-9);
    }

    [Fact]
    public void Solve_UniqueSolution3x3_SatisfiesTheSystemBySubstitution()
    {
        var a = M(new double[,] { { 1, 2, 3 }, { 0, 1, 4 }, { 5, 6, 0 } });
        var expectedX = M(new double[,] { { 1 }, { 2 }, { 3 } });
        var b = a.Multiply(expectedX);

        var solution = LinearSolver.Solve(a, b);

        solution.Kind.Should().Be(SolutionKind.Unique);
        AssertSatisfies(a, solution.Particular!, b);
        for (var i = 0; i < 3; i++)
            solution.Particular![i, 0].Should().BeApproximately(expectedX[i, 0], 1e-9);
    }

    [Fact]
    public void Solve_IncompatibleSystem_ReturnsNone()
    {
        var a = M(new double[,] { { 1, 1 }, { 1, 1 } });
        var b = M(new double[,] { { 1 }, { 2 } });

        var solution = LinearSolver.Solve(a, b);

        solution.Kind.Should().Be(SolutionKind.None);
    }

    [Fact]
    public void Solve_UnderdeterminedSystem_ReturnsInfiniteWithOneFreeVariableAndValidGeneralSolution()
    {
        var a = M(new double[,] { { 1, 1 } });
        var b = M(new double[,] { { 2 } });

        var solution = LinearSolver.Solve(a, b);

        solution.Kind.Should().Be(SolutionKind.Infinite);
        solution.FreeVariables.Should().Be(1);
        solution.NullSpaceBasis.Should().ContainSingle();

        foreach (var t in new[] { -3.0, -1.0, 0.0, 1.0, 5.0 })
        {
            var basis = solution.NullSpaceBasis[0];
            var x = M(new double[,]
            {
                { solution.Particular![0, 0] + t * basis[0, 0] },
                { solution.Particular![1, 0] + t * basis[1, 0] },
            });
            AssertSatisfies(a, x, b);
        }
    }

    [Fact]
    public void Solve_OverdeterminedCompatibleSystem_ReturnsUnique()
    {
        var a = M(new double[,] { { 1, 1 }, { 1, -1 }, { 2, 0 } });
        var expectedX = M(new double[,] { { 2 }, { 1 } });
        var b = a.Multiply(expectedX);

        var solution = LinearSolver.Solve(a, b);

        solution.Kind.Should().Be(SolutionKind.Unique);
        AssertSatisfies(a, solution.Particular!, b);
    }

    [Fact]
    public void Solve_SingularMatrixWithCompatibleRhs_ReturnsInfiniteNotNone()
    {
        var a = M(new double[,] { { 1, 2 }, { 2, 4 } });
        var b = M(new double[,] { { 3 }, { 6 } });

        var solution = LinearSolver.Solve(a, b);

        solution.Kind.Should().Be(SolutionKind.Infinite);
        solution.Rank.Should().Be(1);
        solution.FreeVariables.Should().Be(1);
    }

    [Fact]
    public void Solve_IncompatibleDimensions_Throws()
    {
        var a = M(new double[,] { { 1, 0 }, { 0, 1 } });
        var b = M(new double[,] { { 1 }, { 2 }, { 3 } });

        var act = () => LinearSolver.Solve(a, b);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Solve_ConstantsAsRowVectorOrColumnVector_ProducesTheSameResult()
    {
        var a = M(new double[,] { { 1, 1 }, { 1, -1 } });
        var bColumn = M(new double[,] { { 3 }, { 1 } });
        var bRow = M(new double[,] { { 3, 1 } });

        var fromColumn = LinearSolver.Solve(a, bColumn);
        var fromRow = LinearSolver.Solve(a, bRow);

        fromColumn.Kind.Should().Be(fromRow.Kind);
        fromColumn.Particular![0, 0].Should().BeApproximately(fromRow.Particular![0, 0], 1e-9);
        fromColumn.Particular![1, 0].Should().BeApproximately(fromRow.Particular![1, 0], 1e-9);
    }
}
