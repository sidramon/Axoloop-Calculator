namespace Domain.Calculator.Algorithms;

using Domain.Calculator.Values;

public static class GaussJordan
{
    public static MatrixValue Invert(MatrixValue matrix)
    {
        if (matrix.Rows != matrix.Columns)
            throw new InvalidOperationException("Inverse requires a square matrix.");

        var n = matrix.Rows;

        // Augmented matrix [A | I]
        var aug = new double[n, 2 * n];
        for (var r = 0; r < n; r++)
        {
            for (var c = 0; c < n; c++) aug[r, c] = matrix[r, c];
            aug[r, n + r] = 1;
        }

        // Gauss-Jordan
        for (var col = 0; col < n; col++)
        {
            var pivot = MatrixArrays.FindPivot(aug, n, col);
            if (Math.Abs(aug[pivot, col]) < MatrixArrays.Tolerance)
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

            MatrixArrays.SwapRows(aug, pivot, col, 2 * n);

            var pivotValue = aug[col, col];
            for (var c = 0; c < 2 * n; c++) aug[col, c] /= pivotValue;

            for (var r = 0; r < n; r++)
            {
                if (r == col) continue;
                var factor = aug[r, col];
                for (var c = 0; c < 2 * n; c++)
                    aug[r, c] -= factor * aug[col, c];
            }
        }

        // Extract the right-hand side
        var result = new double[n, n];
        for (var r = 0; r < n; r++)
            for (var c = 0; c < n; c++)
                result[r, c] = aug[r, n + c];
        return new MatrixValue(result);
    }
}
