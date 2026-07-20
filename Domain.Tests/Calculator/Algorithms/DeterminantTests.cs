namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;
using FluentAssertions;

public class DeterminantTests
{
    private static MatrixValue Of(double[,] data) => new(data);

    [Fact]
    public void Compute_OneByOne_ReturnsTheSingleValue()
    {
        var m = Of(new double[,] { { 7 } });

        Determinant.Compute(m).Should().BeApproximately(7, 1e-10);
    }

    [Fact]
    public void Compute_TwoByTwo_ReturnsKnownValue()
    {
        var m = Of(new double[,] { { 1, 2 }, { 3, 4 } });

        Determinant.Compute(m).Should().BeApproximately(-2, 1e-10);
    }

    [Fact]
    public void Compute_ThreeByThree_ReturnsKnownValue()
    {
        var m = Of(new double[,]
        {
            { 6, 1, 1 },
            { 4, -2, 5 },
            { 2, 8, 7 },
        });

        Determinant.Compute(m).Should().BeApproximately(-306, 1e-10);
    }

    [Fact]
    public void Compute_NonSquareMatrix_Throws()
    {
        var m = Of(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });

        var act = () => Determinant.Compute(m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Compute_TriangularMatrix_ReturnsProductOfDiagonal()
    {
        var m = Of(new double[,]
        {
            { 5, 1, 2 },
            { 0, 3, 4 },
            { 0, 0, 2 },
        });

        Determinant.Compute(m).Should().BeApproximately(30, 1e-9);
    }

    [Fact]
    public void Compute_IdentityMatrix_ReturnsOne()
    {
        var m = Of(new double[,]
        {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 1, 0 },
            { 0, 0, 0, 1 },
        });

        Determinant.Compute(m).Should().BeApproximately(1, 1e-9);
    }

    [Fact]
    public void Compute_MatrixRequiringRowSwap_ReturnsCorrectlySignedValue()
    {
        // Column 0 is [0, 1, 6]: the zero at (0,0) forces LU to pivot on row 2, which is
        // exactly the case an unflipped sign gets wrong — omitting the (-1)^swapCount
        // correction would silently return +7 here instead of -7.
        var m = Of(new double[,]
        {
            { 0, 2, 3 },
            { 1, 4, 5 },
            { 6, 7, 8 },
        });

        Determinant.Compute(m).Should().BeApproximately(-7, 1e-9);
    }

    [Fact]
    public void Compute_SingularMatrix_ReturnsZero()
    {
        var m = Of(new double[,]
        {
            { 1, 2, 3 },
            { 2, 4, 6 },
            { 7, 8, 9 },
        });

        Determinant.Compute(m).Should().BeApproximately(0, 1e-9);
    }

    [Fact]
    public void Compute_TwelveByTwelveMatrix_CompletesQuickly()
    {
        // Diagonally dominant, so guaranteed non-singular, and large enough that cofactor
        // expansion (O(n!), roughly 479 million terms at n=12) would take far longer than
        // this test's budget — LU's O(n^3) should finish near-instantly.
        const int n = 12;
        var data = new double[n, n];
        for (var r = 0; r < n; r++)
            for (var c = 0; c < n; c++)
                data[r, c] = r == c ? 20 : 1;
        var m = Of(data);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var det = Determinant.Compute(m);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
        det.Should().NotBe(0);
        double.IsFinite(det).Should().BeTrue();
    }

    [Fact]
    public void Compute_HilbertFourByFour_MatchesKnownValueWithinLooserTolerance()
    {
        // The 4x4 Hilbert matrix is a standard ill-conditioning example (condition number
        // in the tens of thousands), so LU's rounding error is larger here than on the
        // well-conditioned integer cases above — hence the wider tolerance.
        var m = Of(new double[,]
        {
            { 1, 1.0 / 2, 1.0 / 3, 1.0 / 4 },
            { 1.0 / 2, 1.0 / 3, 1.0 / 4, 1.0 / 5 },
            { 1.0 / 3, 1.0 / 4, 1.0 / 5, 1.0 / 6 },
            { 1.0 / 4, 1.0 / 5, 1.0 / 6, 1.0 / 7 },
        });

        // Known closed-form value: det(H4) = 1 / 6048000.
        Determinant.Compute(m).Should().BeApproximately(1.0 / 6048000, 1e-12);
    }
}
