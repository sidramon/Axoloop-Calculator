namespace Domain.Calculator.Algorithms;

using Domain.Calculator.Values;

public static class RowEchelon
{
    public static (double[,] Reduced, int Rank) Reduce(MatrixValue matrix)
    {
        var rows = matrix.Rows;
        var cols = matrix.Columns;
        var m = MatrixArrays.ToArray(matrix);
        var rank = 0;

        for (var col = 0; col < cols && rank < rows; col++)
        {
            var pivot = -1;
            for (var r = rank; r < rows; r++)
                if (Math.Abs(m[r, col]) > MatrixArrays.Tolerance) { pivot = r; break; }

            if (pivot == -1) continue;

            MatrixArrays.SwapRows(m, pivot, rank, cols);

            for (var r = 0; r < rows; r++)
            {
                if (r == rank) continue;
                var factor = m[r, col] / m[rank, col];
                for (var c = col; c < cols; c++)
                    m[r, c] -= factor * m[rank, c];
            }
            rank++;
        }

        return (m, rank);
    }
}
