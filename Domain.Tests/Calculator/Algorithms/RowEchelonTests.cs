namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using Domain.Calculator.Values;
using FluentAssertions;

public class RowEchelonTests
{
    private static MatrixValue Of(double[,] data) => new(data);

    [Fact]
    public void Reduce_FullRankMatrix_ReturnsDimensionAsRank()
    {
        var m = Of(new double[,] { { 1, 0 }, { 0, 1 } });

        RowEchelon.Reduce(m).Rank.Should().Be(2);
    }

    [Fact]
    public void Reduce_RankDeficientMatrix_ReturnsLowerRank()
    {
        var m = Of(new double[,] { { 1, 2 }, { 2, 4 } });

        RowEchelon.Reduce(m).Rank.Should().Be(1);
    }

    [Fact]
    public void Reduce_RectangularMatrix_ReturnsSmallestDimensionWhenFullRank()
    {
        var m = Of(new double[,] { { 1, 0, 0 }, { 0, 1, 0 } });

        RowEchelon.Reduce(m).Rank.Should().Be(2);
    }
}
