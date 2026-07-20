namespace Domain.Calculator.Algorithms;

using Domain.Calculator.Values;

/// <summary>
/// L and U are packed into a single array — the usual convention, since L's diagonal is 1
/// by construction and doesn't need storing. <see cref="Permutation"/>[i] is the original
/// row now at position i (so PA = LU); <see cref="SwapCount"/> is how many row swaps that
/// took, which is what a determinant needs to correct its sign.
/// </summary>
public sealed record LuResult(double[,] Lu, int[] Permutation, int SwapCount, bool IsSingular);

public static class LuDecomposition
{
    public static LuResult Decompose(MatrixValue matrix)
    {
        if (matrix.Rows != matrix.Columns)
            throw new InvalidOperationException("LU decomposition requires a square matrix.");

        var n = matrix.Rows;
        var m = MatrixArrays.ToArray(matrix);
        var permutation = new int[n];
        for (var i = 0; i < n; i++) permutation[i] = i;

        var swapCount = 0;

        for (var col = 0; col < n; col++)
        {
            var pivot = MatrixArrays.FindPivot(m, n, col);
            if (Math.Abs(m[pivot, col]) < MatrixArrays.Tolerance)
            {
                // The column has no usable pivot below the tolerance: the matrix is rank
                // deficient here, so the determinant is zero regardless of what the rest of
                // the elimination would produce. Stop rather than divide by ~0.
                return new LuResult(m, permutation, swapCount, IsSingular: true);
            }

            if (pivot != col)
            {
                MatrixArrays.SwapRows(m, pivot, col, n);
                (permutation[pivot], permutation[col]) = (permutation[col], permutation[pivot]);
                swapCount++;
            }

            for (var r = col + 1; r < n; r++)
            {
                var factor = m[r, col] / m[col, col];
                m[r, col] = factor;
                for (var c = col + 1; c < n; c++)
                    m[r, c] -= factor * m[col, c];
            }
        }

        return new LuResult(m, permutation, swapCount, IsSingular: false);
    }
}
