namespace Domain.Calculator.Algorithms;

using Domain.Calculator.Values;

public static class Determinant
{
    public static double Compute(MatrixValue matrix)
    {
        if (matrix.Rows != matrix.Columns)
            throw new InvalidOperationException("Determinant requires a square matrix.");

        var n = matrix.Rows;
        if (n == 1) return matrix[0, 0];
        if (n == 2) return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

        var lu = LuDecomposition.Decompose(matrix);
        if (lu.IsSingular) return 0;

        var det = 1.0;
        for (var i = 0; i < n; i++) det *= lu.Lu[i, i];

        return lu.SwapCount % 2 == 0 ? det : -det;
    }
}
