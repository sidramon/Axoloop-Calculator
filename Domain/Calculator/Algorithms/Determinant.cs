namespace Domain.Calculator.Algorithms;

using Domain.Calculator.Values;

public static class Determinant
{
    public static double Compute(MatrixValue matrix)
    {
        if (matrix.Rows != matrix.Columns)
            throw new InvalidOperationException("Determinant requires a square matrix.");
        return DeterminantRecursive(matrix.Data, matrix.Rows);
    }

    private static double DeterminantRecursive(double[,] m, int n)
    {
        if (n == 1) return m[0, 0];
        if (n == 2) return m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0];

        double det = 0;
        for (var col = 0; col < n; col++)
        {
            var minor = Minor(m, n, 0, col);
            var sign = col % 2 == 0 ? 1 : -1;
            det += sign * m[0, col] * DeterminantRecursive(minor, n - 1);
        }
        return det;
    }

    private static double[,] Minor(double[,] m, int n, int skipRow, int skipCol)
    {
        var minor = new double[n - 1, n - 1];
        var dr = 0;
        for (var r = 0; r < n; r++)
        {
            if (r == skipRow) continue;
            var dc = 0;
            for (var c = 0; c < n; c++)
            {
                if (c == skipCol) continue;
                minor[dr, dc] = m[r, c];
                dc++;
            }
            dr++;
        }
        return minor;
    }
}
