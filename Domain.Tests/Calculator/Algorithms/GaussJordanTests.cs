namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;
using FluentAssertions;

public class GaussJordanTests
{
    private static MatrixValue Of(double[,] data) => new(data);

    [Fact]
    public void Invert_SquareMatrix_MultipliedByOriginalYieldsIdentity()
    {
        var m = Of(new double[,] { { 4, 7 }, { 2, 6 } });

        var inverse = GaussJordan.Invert(m);
        var product = m.Multiply(inverse);

        for (var r = 0; r < product.Rows; r++)
        for (var c = 0; c < product.Columns; c++)
            product[r, c].Should().BeApproximately(r == c ? 1 : 0, 1e-10);
    }

    [Fact]
    public void Invert_SingularMatrix_Throws()
    {
        var m = Of(new double[,] { { 1, 2 }, { 2, 4 } });

        var act = () => GaussJordan.Invert(m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Invert_NonSquareMatrix_Throws()
    {
        var m = Of(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });

        var act = () => GaussJordan.Invert(m);

        act.Should().Throw<InvalidOperationException>();
    }
}
