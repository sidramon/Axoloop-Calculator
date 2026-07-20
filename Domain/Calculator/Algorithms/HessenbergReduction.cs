namespace Domain.Calculator.Algorithms;

/// <summary>
/// Reduces a matrix to upper Hessenberg form (zero below the first subdiagonal) via
/// Householder reflections applied as similarity transforms, which preserves the
/// spectrum. This is a one-time O(n^3) cost that lets a QR-based eigenvalue iteration
/// exploit the resulting sparsity afterward — though <see cref="EigenDecomposition"/>'s QR
/// step does not yet do so; that is a separate, later change.
/// </summary>
public static class HessenbergReduction
{
    public static double[,] Reduce(double[,] matrix, int size)
    {
        var a = (double[,])matrix.Clone();

        for (var k = 0; k < size - 2; k++)
        {
            var m = size - k - 1;
            var x = new double[m];
            for (var i = 0; i < m; i++) x[i] = a[k + 1 + i, k];

            var tailNormSquared = 0.0;
            for (var i = 1; i < m; i++) tailNormSquared += x[i] * x[i];

            // Everything below the subdiagonal entry x[0] is already (numerically) zero:
            // nothing to annul in this column, and computing a reflector anyway would risk
            // flipping x[0]'s sign for no reason.
            if (Math.Sqrt(tailNormSquared) < MatrixArrays.Tolerance) continue;

            var normX = Math.Sqrt(x[0] * x[0] + tailNormSquared);

            // alpha = -sign(x0)*||x||, not +sign(x0)*||x||: the opposite choice would make
            // v[0] = x0 - alpha subtract two near-equal magnitudes whenever x0 is close to
            // ||x||, losing precision catastrophically. This way v[0] = x0 + sign(x0)*||x||
            // always adds same-signed terms.
            var alpha = x[0] >= 0 ? -normX : normX;

            var v = (double[])x.Clone();
            v[0] -= alpha;

            var normV = 0.0;
            for (var i = 0; i < m; i++) normV += v[i] * v[i];
            normV = Math.Sqrt(normV);
            if (normV < MatrixArrays.Tolerance) continue;

            for (var i = 0; i < m; i++) v[i] /= normV;

            ApplyLeft(a, v, k, size);
            ApplyRight(a, v, k, size);
        }

        return a;
    }

    // A - 2*v*(v^T*A), restricted to the rows v is embedded in (k+1..size-1); O(size^2),
    // never forming the size x size reflector matrix I - 2*v*v^T explicitly.
    private static void ApplyLeft(double[,] a, double[] v, int k, int size)
    {
        var m = v.Length;
        for (var col = 0; col < size; col++)
        {
            var dot = 0.0;
            for (var i = 0; i < m; i++) dot += v[i] * a[k + 1 + i, col];
            var factor = 2 * dot;
            for (var i = 0; i < m; i++) a[k + 1 + i, col] -= factor * v[i];
        }
    }

    // A - 2*(A*v)*v^T, restricted to the columns v is embedded in — this half is what
    // makes the transform a similarity (HAH instead of just HA), so eigenvalues are
    // preserved rather than merely triangularized away.
    private static void ApplyRight(double[,] a, double[] v, int k, int size)
    {
        var m = v.Length;
        for (var row = 0; row < size; row++)
        {
            var dot = 0.0;
            for (var i = 0; i < m; i++) dot += a[row, k + 1 + i] * v[i];
            var factor = 2 * dot;
            for (var i = 0; i < m; i++) a[row, k + 1 + i] -= factor * v[i];
        }
    }
}
