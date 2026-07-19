namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;
using FluentAssertions;

public class DeterminantTests
{
    private static MatrixValue Of(double[,] data) => new(data);

    [Fact]
    public void Compute_OneByOne_ReturnsTheSingleValue()
    {
        var m = Of(new double[,] { { 7 } });

        Determinant.Compute(m).Should().BeApproximately(7, 1e-10);
    }

    [Fact]
    public void Compute_TwoByTwo_ReturnsKnownValue()
    {
        var m = Of(new double[,] { { 1, 2 }, { 3, 4 } });

        Determinant.Compute(m).Should().BeApproximately(-2, 1e-10);
    }

    [Fact]
    public void Compute_ThreeByThree_ReturnsKnownValue()
    {
        var m = Of(new double[,]
        {
            { 6, 1, 1 },
            { 4, -2, 5 },
            { 2, 8, 7 },
        });

        Determinant.Compute(m).Should().BeApproximately(-306, 1e-10);
    }

    [Fact]
    public void Compute_NonSquareMatrix_Throws()
    {
        var m = Of(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });

        var act = () => Determinant.Compute(m);

        act.Should().Throw<InvalidOperationException>();
    }
}
