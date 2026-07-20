namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;
using FluentAssertions;

public class HessenbergReductionTests
{
    private static MatrixValue Of(double[,] data) => new(data);

    [Fact]
    public void Reduce_DenseAsymmetricMatrix_IsZeroBelowFirstSubdiagonal()
    {
        var m = Of(new double[,]
        {
            { 4, 1, 3, 2 },
            { 2, 5, 1, 3 },
            { 1, 2, 6, 4 },
            { 3, 1, 2, 7 },
        });

        var reduced = HessenbergReduction.Reduce(m.Data, 4);

        AssertIsUpperHessenberg(reduced, 4);
    }

    [Fact]
    public void Reduce_DenseAsymmetricMatrix_PreservesTraceAndDeterminant()
    {
        // Trace and determinant are both invariants of a similarity transform (sum and
        // product of eigenvalues respectively) — checking them is a way to confirm the
        // reduction is similarity-preserving without needing the eigenvalue solver at all.
        var m = Of(new double[,]
        {
            { 4, 1, 3, 2 },
            { 2, 5, 1, 3 },
            { 1, 2, 6, 4 },
            { 3, 1, 2, 7 },
        });

        var reduced = Of(HessenbergReduction.Reduce(m.Data, 4));

        Trace(reduced).Should().BeApproximately(Trace(m), 1e-9);
        Determinant.Compute(reduced).Should().BeApproximately(Determinant.Compute(m), 1e-7);
    }

    [Fact]
    public void Reduce_AlreadyHessenbergMatrix_IsUnchanged()
    {
        var m = Of(new double[,]
        {
            { 1, 2, 3, 4 },
            { 5, 6, 7, 8 },
            { 0, 9, 10, 11 },
            { 0, 0, 12, 13 },
        });

        var reduced = HessenbergReduction.Reduce(m.Data, 4);

        for (var r = 0; r < 4; r++)
            for (var c = 0; c < 4; c++)
                reduced[r, c].Should().BeApproximately(m[r, c], 1e-12);
    }

    [Fact]
    public void Reduce_ColumnWithAlreadyZeroTail_SkipsWithoutDivisionByZero()
    {
        // Column 1's tail (position (3,1)) is already zero while the rest of the matrix
        // is dense: the reflector for k=1 must be skipped rather than attempted on a
        // zero-norm tail.
        var m = Of(new double[,]
        {
            { 4, 1, 3, 2 },
            { 2, 5, 1, 3 },
            { 1, 2, 6, 4 },
            { 3, 0, 2, 7 },
        });

        var reduced = HessenbergReduction.Reduce(m.Data, 4);

        for (var r = 0; r < 4; r++)
            for (var c = 0; c < 4; c++)
                double.IsFinite(reduced[r, c]).Should().BeTrue();

        AssertIsUpperHessenberg(reduced, 4);
    }

    [Fact]
    public void Reduce_OneByOne_ReturnsUnchanged()
    {
        var m = Of(new double[,] { { 5 } });

        var reduced = HessenbergReduction.Reduce(m.Data, 1);

        reduced[0, 0].Should().Be(5);
    }

    [Fact]
    public void Reduce_TwoByTwo_ReturnsUnchanged()
    {
        var m = Of(new double[,] { { 1, 2 }, { 3, 4 } });

        var reduced = HessenbergReduction.Reduce(m.Data, 2);

        for (var r = 0; r < 2; r++)
            for (var c = 0; c < 2; c++)
                reduced[r, c].Should().Be(m[r, c]);
    }

    [Fact]
    public void Reduce_IllConditionedColumnWhereWrongReflectorSignWouldCancel_StaysAccurate()
    {
        // x0 = 1 and the tail is tiny (1e-10): a reflector built with the wrong sign
        // (alpha = +sign(x0)*||x|| instead of -sign(x0)*||x||) would compute
        // v[0] = x0 - alpha as a subtraction of two nearly-equal magnitudes, losing most
        // of the tail's information to cancellation. The correct sign choice adds instead
        // of subtracts, so this should reduce cleanly regardless of how tiny the tail is.
        var m = Of(new double[,]
        {
            { 2, 3, 1 },
            { 1, 4, 2 },
            { 1e-10, 1, 5 },
        });

        var reduced = Of(HessenbergReduction.Reduce(m.Data, 3));

        AssertIsUpperHessenberg(reduced.Data, 3);
        Trace(reduced).Should().BeApproximately(Trace(m), 1e-9);
        Determinant.Compute(reduced).Should().BeApproximately(Determinant.Compute(m), 1e-9);
    }

    private static double Trace(MatrixValue m)
    {
        double sum = 0;
        for (var i = 0; i < m.Rows; i++) sum += m[i, i];
        return sum;
    }

    private static void AssertIsUpperHessenberg(double[,] m, int n)
    {
        for (var row = 2; row < n; row++)
            for (var col = 0; col < row - 1; col++)
                m[row, col].Should().BeApproximately(0, 1e-9);
    }
}
