namespace Domain.Calculator.Values;

public sealed record MatrixValue : Value
{
    public double[,] Data { get; }
    public int Rows => Data.GetLength(0);
    public int Columns => Data.GetLength(1);

    public MatrixValue(double[,] data) => Data = data;

    public double this[int row, int col] => Data[row, col];

    public MatrixValue Add(MatrixValue other)
    {
        RequireSameShape(other, "addition");
        return Map((r, c) => this[r, c] + other[r, c]);
    }

    public MatrixValue Subtract(MatrixValue other)
    {
        RequireSameShape(other, "subtraction");
        return Map((r, c) => this[r, c] - other[r, c]);
    }

    public MatrixValue Scale(double scalar) => Map((r, c) => this[r, c] * scalar);

    public MatrixValue Multiply(MatrixValue other)
    {
        if (Columns != other.Rows)
            throw new InvalidOperationException(
                $"Cannot multiply {Rows}x{Columns} by {other.Rows}x{other.Columns}: inner dimensions must match.");

        var result = new double[Rows, other.Columns];
        for (var r = 0; r < Rows; r++)
            for (var c = 0; c < other.Columns; c++)
            {
                double sum = 0;
                for (var k = 0; k < Columns; k++)
                    sum += this[r, k] * other[k, c];
                result[r, c] = sum;
            }
        return new MatrixValue(result);
    }

    public MatrixValue Transpose()
    {
        var result = new double[Columns, Rows];
        for (var r = 0; r < Rows; r++)
            for (var c = 0; c < Columns; c++)
                result[c, r] = this[r, c];
        return new MatrixValue(result);
    }

    private MatrixValue Map(Func<int, int, double> f)
    {
        var result = new double[Rows, Columns];
        for (var r = 0; r < Rows; r++)
            for (var c = 0; c < Columns; c++)
                result[r, c] = f(r, c);
        return new MatrixValue(result);
    }

    private void RequireSameShape(MatrixValue other, string operation)
    {
        if (Rows != other.Rows || Columns != other.Columns)
            throw new InvalidOperationException(
                $"Cannot perform {operation} on {Rows}x{Columns} and {other.Rows}x{other.Columns} matrices.");
    }

    public double Trace()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Trace requires a square matrix.");
        double sum = 0;
        for (var i = 0; i < Rows; i++) sum += this[i, i];
        return sum;
    }

    public static MatrixValue Identity(int size)
    {
        if (size < 1)
            throw new InvalidOperationException("Identity size must be at least 1.");
        var data = new double[size, size];
        for (var i = 0; i < size; i++) data[i, i] = 1;
        return new MatrixValue(data);
    }

    public double Dot(MatrixValue other)
    {
        var a = AsVector("dot product");
        var b = other.AsVector("dot product");

        if (a.Length != b.Length)
            throw new InvalidOperationException(
                $"Dot product requires vectors of equal length ({a.Length} vs {b.Length}).");

        double sum = 0;
        for (var i = 0; i < a.Length; i++) sum += a[i] * b[i];
        return sum;
    }

    public MatrixValue Cross(MatrixValue other)
    {
        var a = AsVector("cross product");
        var b = other.AsVector("cross product");

        if (a.Length != 3 || b.Length != 3)
            throw new InvalidOperationException("Cross product requires two 3-element vectors.");

        var result = new[]
        {
            a[1] * b[2] - a[2] * b[1],
            a[2] * b[0] - a[0] * b[2],
            a[0] * b[1] - a[1] * b[0],
        };
        
        if (Columns == 1)
        {
            var column = new double[3, 1];
            for (var i = 0; i < 3; i++) column[i, 0] = result[i];
            return new MatrixValue(column);
        }

        var row = new double[1, 3];
        for (var i = 0; i < 3; i++) row[0, i] = result[i];
        return new MatrixValue(row);
    }

    private double[] AsVector(string operation)
    {
        if (Rows != 1 && Columns != 1)
            throw new InvalidOperationException(
                $"{operation} requires a vector (1xN or Nx1), got {Rows}x{Columns}.");

        var length = Rows == 1 ? Columns : Rows;
        var vector = new double[length];
        for (var i = 0; i < length; i++)
            vector[i] = Rows == 1 ? this[0, i] : this[i, 0];
        return vector;
    }
    
    public static MatrixValue Filled(int rows, int columns, double value)
    {
        if (rows < 1 || columns < 1)
            throw new InvalidOperationException("Matrix dimensions must be at least 1x1.");

        var data = new double[rows, columns];
        if (value != 0)
            for (var r = 0; r < rows; r++)
            for (var c = 0; c < columns; c++)
                data[r, c] = value;
        return new MatrixValue(data);
    }

    public MatrixValue Reshape(int rows, int columns)
    {
        if (rows < 1 || columns < 1)
            throw new InvalidOperationException("Matrix dimensions must be at least 1x1.");

        var total = Rows * Columns;
        if (rows * columns != total)
            throw new InvalidOperationException(
                $"Cannot reshape {Rows}x{Columns} ({total} elements) into {rows}x{columns} ({rows * columns} elements).");

        var data = new double[rows, columns];
        for (var i = 0; i < total; i++)
            data[i / columns, i % columns] = this[i / Columns, i % Columns];
        return new MatrixValue(data);
    }
}