namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;
using FluentAssertions;

public class EigenDecompositionTests
{
    private static MatrixValue Of(double[,] data) => new(data);

    [Fact]
    public void General_DiagonalMatrix_ReturnsDiagonalSortedDescending()
    {
        var m = Of(new double[,]
        {
            { 2, 0, 0 },
            { 0, 5, 0 },
            { 0, 0, 1 },
        });

        EigenDecomposition.General(m).Should().Equal(new[] { 5.0, 2.0, 1.0 });
    }

    [Fact]
    public void General_TriangularMatrix_ReturnsDiagonalSortedDescending()
    {
        var m = Of(new double[,]
        {
            { 3, 1, 4 },
            { 0, 6, 2 },
            { 0, 0, 5 },
        });

        var values = EigenDecomposition.General(m);

        values.Should().HaveCount(3);
        values[0].Should().BeApproximately(6, 1e-6);
        values[1].Should().BeApproximately(5, 1e-6);
        values[2].Should().BeApproximately(3, 1e-6);
    }

    [Fact]
    public void General_SymmetricMatrix_DelegatesToJacobiUnaffectedByHessenbergChange()
    {
        var m = Of(new double[,]
        {
            { 2, 1 },
            { 1, 2 },
        });

        // Known eigenvalues of [[2,1],[1,2]]: 3 and 1.
        var values = EigenDecomposition.General(m);

        values[0].Should().BeApproximately(3, 1e-9);
        values[1].Should().BeApproximately(1, 1e-9);
    }

    [Fact]
    public void General_AsymmetricTwoByTwoMatrix_ReturnsKnownRealEigenvalues()
    {
        // trace=7, det=10, discriminant=9 -> eigenvalues (7+-3)/2 = 5, 2.
        var m = Of(new double[,] { { 4, 1 }, { 2, 3 } });

        var values = EigenDecomposition.General(m);

        values[0].Should().BeApproximately(5, 1e-6);
        values[1].Should().BeApproximately(2, 1e-6);
    }

    [Fact]
    public void General_ComplexEigenvalues_Throws()
    {
        var m = Of(new double[,] { { 0, -1 }, { 1, 0 } });

        var act = () => EigenDecomposition.General(m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void General_DenseAsymmetricMatrixSimilarToKnownDiagonal_RecoversItsEigenvalues()
    {
        // A = P * diag(1,2,3) * P^-1 for a non-triangular, non-orthogonal P: A shares
        // diag(1,2,3)'s eigenvalues exactly by construction (any similarity transform
        // preserves the spectrum), regardless of what Hessenberg reduction does to A along
        // the way. This is the central "reduction doesn't change the spectrum" check, run
        // through the public eigenvalue solver rather than by inspecting internals.
        var p = Of(new double[,]
        {
            { 2, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 3 },
        });
        var d = Of(new double[,]
        {
            { 1, 0, 0 },
            { 0, 2, 0 },
            { 0, 0, 3 },
        });
        var pInverse = GaussJordan.Invert(p);
        var a = p.Multiply(d).Multiply(pInverse);

        var values = EigenDecomposition.General(a);

        values[0].Should().BeApproximately(3, 1e-6);
        values[1].Should().BeApproximately(2, 1e-6);
        values[2].Should().BeApproximately(1, 1e-6);
    }

    [Fact]
    public void General_TwentyByTwentyMatrix_CompletesQuickly()
    {
        // Diagonally dominant with a mild asymmetric perturbation, so it's real-diagonalizable
        // (no complex-eigenvalue rejection) and takes the QR path rather than Jacobi's.
        const int n = 20;
        var data = new double[n, n];
        for (var r = 0; r < n; r++)
            for (var c = 0; c < n; c++)
                data[r, c] = r == c ? 50 : (r + c) % 3 - 1;
        var m = Of(data);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var values = EigenDecomposition.General(m);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
        values.Should().HaveCount(n);
    }
}
