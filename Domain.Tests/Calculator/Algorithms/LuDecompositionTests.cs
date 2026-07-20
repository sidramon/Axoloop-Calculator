namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;
using FluentAssertions;

public class LuDecompositionTests
{
    private static MatrixValue Of(double[,] data) => new(data);

    [Fact]
    public void Decompose_MatrixRequiringPivoting_ReconstructsPermutedOriginalWithinTolerance()
    {
        // Column 0 is [0, 2, 1]: the zero forces a pivot swap on the very first step.
        var m = Of(new double[,]
        {
            { 0, 1, 1 },
            { 2, 1, 0 },
            { 1, 1, 2 },
        });

        var result = LuDecomposition.Decompose(m);

        result.IsSingular.Should().BeFalse();
        result.SwapCount.Should().BeGreaterThan(0);
        AssertReconstructsPermutedOriginal(m, result);
    }

    [Fact]
    public void Decompose_WellConditionedMatrixWithoutPivoting_ReconstructsOriginalWithinTolerance()
    {
        var m = Of(new double[,]
        {
            { 4, 3 },
            { 6, 3 },
        });

        var result = LuDecomposition.Decompose(m);

        AssertReconstructsPermutedOriginal(m, result);
    }

    [Fact]
    public void Decompose_SingularMatrix_ReportsSingularWithoutThrowing()
    {
        var m = Of(new double[,] { { 1, 2 }, { 2, 4 } });

        var act = () => LuDecomposition.Decompose(m);

        act.Should().NotThrow();
        LuDecomposition.Decompose(m).IsSingular.Should().BeTrue();
    }

    [Fact]
    public void Decompose_NonSquareMatrix_Throws()
    {
        var m = Of(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });

        var act = () => LuDecomposition.Decompose(m);

        act.Should().Throw<InvalidOperationException>();
    }

    private static void AssertReconstructsPermutedOriginal(MatrixValue original, LuResult result)
    {
        var n = original.Rows;
        var (l, u) = SplitLu(result.Lu, n);

        for (var row = 0; row < n; row++)
        {
            for (var col = 0; col < n; col++)
            {
                var reconstructed = 0.0;
                for (var k = 0; k < n; k++) reconstructed += l[row, k] * u[k, col];

                var permutedOriginal = original[result.Permutation[row], col];
                reconstructed.Should().BeApproximately(permutedOriginal, 1e-9);
            }
        }
    }

    private static (double[,] L, double[,] U) SplitLu(double[,] lu, int n)
    {
        var l = new double[n, n];
        var u = new double[n, n];
        for (var r = 0; r < n; r++)
        {
            l[r, r] = 1;
            for (var c = 0; c < n; c++)
            {
                if (c < r) l[r, c] = lu[r, c];
                else u[r, c] = lu[r, c];
            }
        }
        return (l, u);
    }
}
