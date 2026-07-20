namespace Domain.Calculator.Algorithms;

using Domain.Calculator.Values;

public static class LinearSolver
{
    public static LinearSolution Solve(MatrixValue coefficients, MatrixValue constants)
    {
        var rows = coefficients.Rows;
        var columns = coefficients.Columns;
        var b = ExtractVector(constants);

        if (b.Length != rows)
            throw new InvalidOperationException(
                $"Cannot solve a system with {rows}x{columns} coefficients and a constants vector of " +
                $"length {b.Length}: the constants vector must have exactly {rows} entries (one per equation).");

        var augmentedData = new double[rows, columns + 1];
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < columns; c++) augmentedData[r, c] = coefficients[r, c];
            augmentedData[r, columns] = b[r];
        }

        var coefficientRank = RowEchelon.Reduce(coefficients).Rank;
        var (rref, augmentedRank) = RowEchelon.ReduceFully(new MatrixValue(augmentedData));

        if (coefficientRank < augmentedRank)
            return new LinearSolution(
                SolutionKind.None, null, Array.Empty<MatrixValue>(), coefficientRank, columns - coefficientRank);

        var (pivotColumns, freeColumns) = FindPivotsAndFreeColumns(rref, columns, coefficientRank);

        var particularData = new double[columns, 1];
        for (var row = 0; row < coefficientRank; row++)
            particularData[pivotColumns[row], 0] = rref[row, columns];
        var particular = new MatrixValue(particularData);

        if (freeColumns.Count == 0)
            return new LinearSolution(SolutionKind.Unique, particular, Array.Empty<MatrixValue>(), coefficientRank, 0);

        var nullSpaceBasis = new List<MatrixValue>();
        foreach (var freeColumn in freeColumns)
        {
            var vectorData = new double[columns, 1];
            vectorData[freeColumn, 0] = 1;
            for (var row = 0; row < coefficientRank; row++)
                vectorData[pivotColumns[row], 0] = -rref[row, freeColumn];
            nullSpaceBasis.Add(new MatrixValue(vectorData));
        }

        return new LinearSolution(SolutionKind.Infinite, particular, nullSpaceBasis, coefficientRank, freeColumns.Count);
    }

    private static (int[] PivotColumns, List<int> FreeColumns) FindPivotsAndFreeColumns(
        double[,] rref, int columnCount, int rank)
    {
        var pivotColumns = new int[rank];
        var isPivotColumn = new bool[columnCount];

        for (var row = 0; row < rank; row++)
        {
            var col = 0;
            while (col < columnCount && Math.Abs(rref[row, col]) < MatrixArrays.Tolerance) col++;
            pivotColumns[row] = col;
            isPivotColumn[col] = true;
        }

        var freeColumns = new List<int>();
        for (var col = 0; col < columnCount; col++)
            if (!isPivotColumn[col]) freeColumns.Add(col);

        return (pivotColumns, freeColumns);
    }

    private static double[] ExtractVector(MatrixValue vector)
    {
        if (vector.Rows != 1 && vector.Columns != 1)
            throw new InvalidOperationException(
                $"The constants must be a vector (1xN or Nx1), got {vector.Rows}x{vector.Columns}.");

        var length = vector.Rows == 1 ? vector.Columns : vector.Rows;
        var result = new double[length];
        for (var i = 0; i < length; i++)
            result[i] = vector.Rows == 1 ? vector[0, i] : vector[i, 0];
        return result;
    }
}
