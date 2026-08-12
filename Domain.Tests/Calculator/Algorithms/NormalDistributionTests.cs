namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using FluentAssertions;

public class NormalDistributionTests
{
    [Fact]
    public void Pdf_StandardNormalAtZero_ReturnsOneOverSqrtTwoPi()
    {
        NormalDistribution.Pdf(0, 0, 1).Should().BeApproximately(1 / Math.Sqrt(2 * Math.PI), 1e-9);
    }

    [Fact]
    public void Pdf_NonPositiveSigma_Throws()
    {
        var act = () => NormalDistribution.Pdf(0, 0, 0);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cdf_AtTheMean_ReturnsOneHalf()
    {
        NormalDistribution.Cdf(0, 0, 1).Should().BeApproximately(0.5, 1e-6);
    }

    [Fact]
    public void Cdf_ArbitraryMeanAtTheMean_StillReturnsOneHalf()
    {
        NormalDistribution.Cdf(17, 17, 3).Should().BeApproximately(0.5, 1e-6);
    }

    [Theory]
    [InlineData(1, 0.8413447)] // one standard deviation above the mean
    [InlineData(-1, 0.1586553)] // one standard deviation below the mean
    [InlineData(2, 0.9772499)] // two standard deviations above the mean
    [InlineData(-2, 0.0227501)] // two standard deviations below the mean
    public void Cdf_StandardDeviationMultiples_MatchTabulatedValues(double x, double expected)
    {
        NormalDistribution.Cdf(x, 0, 1).Should().BeApproximately(expected, 1e-6);
    }

    [Fact]
    public void Cdf_NonPositiveSigma_Throws()
    {
        var act = () => NormalDistribution.Cdf(0, 0, -1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(0.1)]
    [InlineData(0.9)]
    [InlineData(0.8413447)]
    public void InverseCdf_RoundTripsThroughCdf(double x)
    {
        var p = NormalDistribution.Cdf(x, 0, 1);
        var recovered = NormalDistribution.InverseCdf(p, 0, 1);

        recovered.Should().BeApproximately(x, 1e-6);
    }

    [Fact]
    public void InverseCdf_OneHalf_ReturnsTheMean()
    {
        NormalDistribution.InverseCdf(0.5, 7, 2).Should().BeApproximately(7, 1e-6);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void InverseCdf_POutsideOpenUnitInterval_Throws(double p)
    {
        var act = () => NormalDistribution.InverseCdf(p, 0, 1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void InverseCdf_NonPositiveSigma_Throws()
    {
        var act = () => NormalDistribution.InverseCdf(0.5, 0, 0);

        act.Should().Throw<InvalidOperationException>();
    }
}
