namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using FluentAssertions;

public class BinomialDistributionTests
{
    [Theory]
    [InlineData(0, 4, 0.5, 0.0625)]
    [InlineData(2, 4, 0.5, 0.375)]
    [InlineData(4, 4, 0.5, 0.0625)]
    public void Pmf_FairCoinFourFlips_MatchesHandComputedValues(int k, int n, double p, double expected)
    {
        BinomialDistribution.Pmf(k, n, p).Should().BeApproximately(expected, 1e-9);
    }

    [Fact]
    public void Pmf_SumOverAllK_EqualsOne()
    {
        const int n = 10;
        const double p = 0.3;

        var total = Enumerable.Range(0, n + 1).Sum(k => BinomialDistribution.Pmf(k, n, p));

        total.Should().BeApproximately(1, 1e-9);
    }

    [Fact]
    public void Pmf_NegativeN_Throws()
    {
        var act = () => BinomialDistribution.Pmf(1, -1, 0.5);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Pmf_POutsideZeroOneRange_Throws(double p)
    {
        var act = () => BinomialDistribution.Pmf(1, 5, p);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Pmf_KGreaterThanN_Throws()
    {
        var act = () => BinomialDistribution.Pmf(6, 5, 0.5);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Pmf_NegativeK_Throws()
    {
        var act = () => BinomialDistribution.Pmf(-1, 5, 0.5);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cdf_SumOfPmfsUpToK_MatchesHandComputedValue()
    {
        // P(0) + P(1) + P(2) for n=4, p=0.5 = 0.0625 + 0.25 + 0.375 = 0.6875
        BinomialDistribution.Cdf(2, 4, 0.5).Should().BeApproximately(0.6875, 1e-9);
    }

    [Fact]
    public void Cdf_AtK_EqualsN_ReturnsOne()
    {
        BinomialDistribution.Cdf(4, 4, 0.5).Should().BeApproximately(1, 1e-9);
    }
}
