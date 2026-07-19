namespace Domain.Tests.Calculator.Values;

using Domain.Calculator.Values;
using FluentAssertions;

public class MatrixValueTests
{
    private static MatrixValue Of(double[,] data) => new(data);

    // ---- Multiply ----

    [Fact]
    public void Multiply_CompatibleDimensions_ReturnsProduct()
    {
        var a = Of(new double[,] { { 1, 2 }, { 3, 4 } });
        var b = Of(new double[,] { { 5, 6 }, { 7, 8 } });

        var result = a.Multiply(b);

        result[0, 0].Should().BeApproximately(19, 1e-10);
        result[0, 1].Should().BeApproximately(22, 1e-10);
        result[1, 0].Should().BeApproximately(43, 1e-10);
        result[1, 1].Should().BeApproximately(50, 1e-10);
    }

    [Fact]
    public void Multiply_IncompatibleDimensions_Throws()
    {
        var a = Of(new double[,] { { 1, 2, 3 } });
        var b = Of(new double[,] { { 1, 2, 3 } });

        var act = () => a.Multiply(b);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiply_IsNotCommutative()
    {
        var a = Of(new double[,] { { 1, 2 }, { 3, 4 } });
        var b = Of(new double[,] { { 0, 1 }, { 1, 0 } });

        var ab = a.Multiply(b);
        var ba = b.Multiply(a);

        ab[0, 0].Should().NotBe(ba[0, 0]);
    }

    // ---- Add / Subtract ----

    [Fact]
    public void Add_SameDimensions_ReturnsElementwiseSum()
    {
        var a = Of(new double[,] { { 1, 2 }, { 3, 4 } });
        var b = Of(new double[,] { { 5, 6 }, { 7, 8 } });

        var result = a.Add(b);

        result[0, 0].Should().BeApproximately(6, 1e-10);
        result[1, 1].Should().BeApproximately(12, 1e-10);
    }

    [Fact]
    public void Add_DifferentDimensions_Throws()
    {
        var a = Of(new double[,] { { 1, 2 } });
        var b = Of(new double[,] { { 1 }, { 2 } });

        var act = () => a.Add(b);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtract_SameDimensions_ReturnsElementwiseDifference()
    {
        var a = Of(new double[,] { { 5, 6 }, { 7, 8 } });
        var b = Of(new double[,] { { 1, 2 }, { 3, 4 } });

        var result = a.Subtract(b);

        result[0, 0].Should().BeApproximately(4, 1e-10);
        result[1, 1].Should().BeApproximately(4, 1e-10);
    }

    [Fact]
    public void Subtract_DifferentDimensions_Throws()
    {
        var a = Of(new double[,] { { 1, 2 } });
        var b = Of(new double[,] { { 1 }, { 2 } });

        var act = () => a.Subtract(b);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Reshape ----

    [Fact]
    public void Reshape_CorrectElementCount_PreservesRowMajorOrder()
    {
        var m = Of(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });

        var reshaped = m.Reshape(3, 2);

        reshaped[0, 0].Should().Be(1);
        reshaped[0, 1].Should().Be(2);
        reshaped[1, 0].Should().Be(3);
        reshaped[1, 1].Should().Be(4);
        reshaped[2, 0].Should().Be(5);
        reshaped[2, 1].Should().Be(6);
    }

    [Fact]
    public void Reshape_IncorrectElementCount_Throws()
    {
        var m = Of(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });

        var act = () => m.Reshape(2, 2);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Dot ----

    [Fact]
    public void Dot_RowVectors_ReturnsSumOfProducts()
    {
        var a = Of(new double[,] { { 1, 2, 3 } });
        var b = Of(new double[,] { { 4, 5, 6 } });

        a.Dot(b).Should().BeApproximately(32, 1e-10);
    }

    [Fact]
    public void Dot_ColumnVectors_ReturnsSumOfProducts()
    {
        var a = Of(new double[,] { { 1 }, { 2 }, { 3 } });
        var b = Of(new double[,] { { 4 }, { 5 }, { 6 } });

        a.Dot(b).Should().BeApproximately(32, 1e-10);
    }

    [Fact]
    public void Dot_DifferentLengths_Throws()
    {
        var a = Of(new double[,] { { 1, 2, 3 } });
        var b = Of(new double[,] { { 1, 2 } });

        var act = () => a.Dot(b);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Cross ----

    [Fact]
    public void Cross_ThreeDimensionalVectors_ReturnsKnownResult()
    {
        var a = Of(new double[,] { { 1, 0, 0 } });
        var b = Of(new double[,] { { 0, 1, 0 } });

        var result = a.Cross(b);

        result[0, 0].Should().BeApproximately(0, 1e-10);
        result[0, 1].Should().BeApproximately(0, 1e-10);
        result[0, 2].Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Cross_ResultOrientation_FollowsLeftOperand()
    {
        var rowLeft = Of(new double[,] { { 1, 0, 0 } });
        var columnRight = Of(new double[,] { { 0 }, { 1 }, { 0 } });

        var result = rowLeft.Cross(columnRight);

        result.Rows.Should().Be(1);
        result.Columns.Should().Be(3);
    }

    [Fact]
    public void Cross_ColumnLeftOperand_ReturnsColumnResult()
    {
        var columnLeft = Of(new double[,] { { 1 }, { 0 }, { 0 } });
        var rowRight = Of(new double[,] { { 0, 1, 0 } });

        var result = columnLeft.Cross(rowRight);

        result.Rows.Should().Be(3);
        result.Columns.Should().Be(1);
    }

    [Fact]
    public void Cross_NonThreeDimensionalVectors_Throws()
    {
        var a = Of(new double[,] { { 1, 0 } });
        var b = Of(new double[,] { { 0, 1 } });

        var act = () => a.Cross(b);

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Trace / Transpose / Scale / Identity / Filled ----

    [Fact]
    public void Trace_SquareMatrix_ReturnsSumOfDiagonal()
    {
        var m = Of(new double[,] { { 1, 2 }, { 3, 4 } });

        m.Trace().Should().BeApproximately(5, 1e-10);
    }

    [Fact]
    public void Trace_NonSquareMatrix_Throws()
    {
        var m = Of(new double[,] { { 1, 2, 3 } });

        var act = () => m.Trace();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Transpose_RectangularMatrix_SwapsRowsAndColumns()
    {
        var m = Of(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });

        var t = m.Transpose();

        t.Rows.Should().Be(3);
        t.Columns.Should().Be(2);
        t[0, 0].Should().Be(1);
        t[0, 1].Should().Be(4);
        t[2, 1].Should().Be(6);
    }

    [Fact]
    public void Scale_ByFactor_MultipliesEveryElement()
    {
        var m = Of(new double[,] { { 1, 2 }, { 3, 4 } });

        var scaled = m.Scale(2);

        scaled[0, 0].Should().BeApproximately(2, 1e-10);
        scaled[1, 1].Should().BeApproximately(8, 1e-10);
    }

    [Fact]
    public void Identity_GivenSize_ReturnsMatrixWithOnesOnDiagonal()
    {
        var identity = MatrixValue.Identity(3);

        for (var r = 0; r < 3; r++)
        for (var c = 0; c < 3; c++)
            identity[r, c].Should().Be(r == c ? 1 : 0);
    }

    [Fact]
    public void Identity_SizeLessThanOne_Throws()
    {
        var act = () => MatrixValue.Identity(0);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Filled_GivenValue_FillsEveryCell()
    {
        var filled = MatrixValue.Filled(2, 3, 5);

        filled.Rows.Should().Be(2);
        filled.Columns.Should().Be(3);
        for (var r = 0; r < 2; r++)
        for (var c = 0; c < 3; c++)
            filled[r, c].Should().Be(5);
    }
}
