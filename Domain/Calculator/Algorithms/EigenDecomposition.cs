using Domain.Calculator.Values;

namespace Domain.Calculator.Algorithms;

public static class EigenDecomposition
{
    private const int MaxSweeps = 100;
    private const int MaxIterations = 500;

    public static bool IsSymmetric(MatrixValue matrix)
    {
        if (matrix.Rows != matrix.Columns) return false;

        for (var r = 0; r < matrix.Rows; r++)
            for (var c = r + 1; c < matrix.Columns; c++)
                if (Math.Abs(matrix[r, c] - matrix[c, r]) > 1e-10)
                    return false;

        return true;
    }

    public static (double[] Values, double[,] Vectors) Symmetric(MatrixValue matrix)
    {
        RequireSquare(matrix);

        var n = matrix.Rows;
        var a = MatrixArrays.ToArray(matrix);
        var v = new double[n, n];
        for (var i = 0; i < n; i++) v[i, i] = 1;

        for (var sweep = 0; sweep < MaxSweeps; sweep++)
        {
            var off = 0.0;
            for (var p = 0; p < n; p++)
                for (var q = p + 1; q < n; q++)
                    off += a[p, q] * a[p, q];

            if (off < MatrixArrays.Tolerance) break;

            for (var p = 0; p < n; p++)
            {
                for (var q = p + 1; q < n; q++)
                {
                    if (Math.Abs(a[p, q]) < MatrixArrays.Tolerance) continue;

                    var theta = (a[q, q] - a[p, p]) / (2 * a[p, q]);
                    var sign = theta >= 0 ? 1.0 : -1.0;
                    var t = sign / (Math.Abs(theta) + Math.Sqrt(theta * theta + 1));
                    var c = 1 / Math.Sqrt(t * t + 1);
                    var s = t * c;

                    for (var k = 0; k < n; k++)
                    {
                        var akp = a[k, p];
                        var akq = a[k, q];
                        a[k, p] = c * akp - s * akq;
                        a[k, q] = s * akp + c * akq;
                    }

                    for (var k = 0; k < n; k++)
                    {
                        var apk = a[p, k];
                        var aqk = a[q, k];
                        a[p, k] = c * apk - s * aqk;
                        a[q, k] = s * apk + c * aqk;
                    }

                    for (var k = 0; k < n; k++)
                    {
                        var vkp = v[k, p];
                        var vkq = v[k, q];
                        v[k, p] = c * vkp - s * vkq;
                        v[k, q] = s * vkp + c * vkq;
                    }
                }
            }
        }

        var values = new double[n];
        for (var i = 0; i < n; i++) values[i] = a[i, i];

        SortDescending(values, v);
        return (values, v);
    }

    public static double[] General(MatrixValue matrix)
    {
        RequireSquare(matrix);

        if (IsSymmetric(matrix))
            return Symmetric(matrix).Values;

        var n = matrix.Rows;
        var a = MatrixArrays.ToArray(matrix);
        var found = new List<double>();
        var high = n - 1;
        var iterations = 0;

        while (high >= 0)
        {
            if (high == 0)
            {
                found.Add(a[0, 0]);
                break;
            }

            if (SubdiagonalNegligible(a, high))
            {
                found.Add(a[high, high]);
                high--;
                iterations = 0;
                continue;
            }

            if (high == 1 || SubdiagonalNegligible(a, high - 1))
            {
                var p = a[high - 1, high - 1];
                var q = a[high - 1, high];
                var r = a[high, high - 1];
                var s = a[high, high];

                var trace = p + s;
                var discriminant = trace * trace / 4 - (p * s - q * r);

                if (discriminant < -MatrixArrays.Tolerance)
                    throw new InvalidOperationException(
                        "Matrix has complex eigenvalues; only real eigenvalues are supported.");

                var root = Math.Sqrt(Math.Max(discriminant, 0));
                found.Add(trace / 2 + root);
                found.Add(trace / 2 - root);
                high -= 2;
                iterations = 0;
                continue;
            }

            if (++iterations > MaxIterations)
                throw new InvalidOperationException("Eigenvalue iteration did not converge.");

            var shift = WilkinsonShift(a, high);
            var size = high + 1;

            for (var i = 0; i < size; i++) a[i, i] -= shift;
            QrStep(a, size);
            for (var i = 0; i < size; i++) a[i, i] += shift;
        }

        var values = found.ToArray();
        Array.Sort(values);
        Array.Reverse(values);
        return values;
    }

    private static void QrStep(double[,] a, int size)
    {
        var q = new double[size, size];
        var r = new double[size, size];

        for (var j = 0; j < size; j++)
        {
            var column = new double[size];
            for (var i = 0; i < size; i++) column[i] = a[i, j];

            for (var k = 0; k < j; k++)
            {
                double dot = 0;
                for (var i = 0; i < size; i++) dot += q[i, k] * a[i, j];
                r[k, j] = dot;
                for (var i = 0; i < size; i++) column[i] -= dot * q[i, k];
            }

            double norm = 0;
            for (var i = 0; i < size; i++) norm += column[i] * column[i];
            norm = Math.Sqrt(norm);
            r[j, j] = norm;

            if (norm < MatrixArrays.Tolerance)
            {
                for (var i = 0; i < size; i++) q[i, j] = 0;
                continue;
            }

            for (var i = 0; i < size; i++) q[i, j] = column[i] / norm;
        }

        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                double sum = 0;
                for (var k = 0; k < size; k++) sum += r[i, k] * q[k, j];
                a[i, j] = sum;
            }
        }
    }

    private static double WilkinsonShift(double[,] a, int high)
    {
        var p = a[high - 1, high - 1];
        var q = a[high - 1, high];
        var r = a[high, high - 1];
        var s = a[high, high];

        var d = (p - s) / 2;
        var under = d * d + q * r;

        if (under < 0) return s;

        var sign = d >= 0 ? 1.0 : -1.0;
        var denominator = Math.Abs(d) + Math.Sqrt(under);

        return denominator < MatrixArrays.Tolerance ? s : s - sign * q * r / denominator;
    }

    private static bool SubdiagonalNegligible(double[,] a, int index)
    {
        if (index <= 0) return true;
        var scale = Math.Abs(a[index - 1, index - 1]) + Math.Abs(a[index, index]);
        return Math.Abs(a[index, index - 1]) < MatrixArrays.Tolerance * (scale + MatrixArrays.Tolerance);
    }

    private static void SortDescending(double[] values, double[,] vectors)
    {
        var n = values.Length;

        for (var i = 0; i < n - 1; i++)
        {
            var max = i;
            for (var j = i + 1; j < n; j++)
                if (values[j] > values[max]) max = j;

            if (max == i) continue;

            (values[i], values[max]) = (values[max], values[i]);
            for (var k = 0; k < n; k++)
                (vectors[k, i], vectors[k, max]) = (vectors[k, max], vectors[k, i]);
        }
    }

    private static void RequireSquare(MatrixValue matrix)
    {
        if (matrix.Rows != matrix.Columns)
            throw new InvalidOperationException("Eigenvalues require a square matrix.");
    }
}