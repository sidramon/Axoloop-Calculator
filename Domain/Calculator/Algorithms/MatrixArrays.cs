namespace Domain.Calculator.Algorithms;

using Domain.Calculator.Values;

internal static class MatrixArrays
{
    public const double Tolerance = 1e-12;

    public static double[,] ToArray(MatrixValue matrix)
    {
        var data = new double[matrix.Rows, matrix.Columns];
        for (var r = 0; r < matrix.Rows; r++)
            for (var c = 0; c < matrix.Columns; c++)
                data[r, c] = matrix[r, c];
        return data;
    }

    public static void SwapRows(double[,] m, int r1, int r2, int width)
    {
        if (r1 == r2) return;
        for (var c = 0; c < width; c++)
            (m[r1, c], m[r2, c]) = (m[r2, c], m[r1, c]);
    }

    public static int FindPivot(double[,] m, int size, int col)
    {
        var pivot = col;
        var max = Math.Abs(m[col, col]);
        for (var r = col + 1; r < size; r++)
        {
            if (Math.Abs(m[r, col]) > max)
            {
                max = Math.Abs(m[r, col]);
                pivot = r;
            }
        }
        return pivot;
    }
}
