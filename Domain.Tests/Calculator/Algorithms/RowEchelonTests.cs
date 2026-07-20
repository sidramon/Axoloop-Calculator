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

    [Fact]
    public void ReduceFully_RankDeficientMatrix_NormalizesPivotAndZeroesDependentRow()
    {
        var m = Of(new double[,] { { 1, 2 }, { 2, 4 } });

        var (reduced, rank) = RowEchelon.ReduceFully(m);

        rank.Should().Be(1);
        reduced[0, 0].Should().BeApproximately(1, 1e-9);
        reduced[0, 1].Should().BeApproximately(2, 1e-9);
        reduced[1, 0].Should().BeApproximately(0, 1e-9);
        reduced[1, 1].Should().BeApproximately(0, 1e-9);
    }

    [Fact]
    public void ReduceFully_InvertibleMatrix_ReturnsIdentity()
    {
        var m = Of(new double[,] { { 2, 4 }, { 1, 3 } });

        var (reduced, rank) = RowEchelon.ReduceFully(m);

        rank.Should().Be(2);
        reduced[0, 0].Should().BeApproximately(1, 1e-9);
        reduced[0, 1].Should().BeApproximately(0, 1e-9);
        reduced[1, 0].Should().BeApproximately(0, 1e-9);
        reduced[1, 1].Should().BeApproximately(1, 1e-9);
    }
}
