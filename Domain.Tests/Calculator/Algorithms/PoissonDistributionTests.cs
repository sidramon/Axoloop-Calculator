namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using FluentAssertions;

public class PoissonDistributionTests
{
    [Theory]
    [InlineData(0, 3, 0.0497871)]
    [InlineData(3, 3, 0.2240418)]
    public void Pmf_KnownLambda_MatchesHandComputedValues(int k, double lambda, double expected)
    {
        PoissonDistribution.Pmf(k, lambda).Should().BeApproximately(expected, 1e-6);
    }

    [Fact]
    public void Pmf_SumOverManyK_ApproachesOne()
    {
        const double lambda = 4;

        var total = Enumerable.Range(0, 50).Sum(k => PoissonDistribution.Pmf(k, lambda));

        total.Should().BeApproximately(1, 1e-9);
    }

    [Fact]
    public void Pmf_NonPositiveLambda_Throws()
    {
        var act = () => PoissonDistribution.Pmf(1, 0);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Pmf_NegativeK_Throws()
    {
        var act = () => PoissonDistribution.Pmf(-1, 3);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cdf_SumOfPmfsUpToK_MatchesHandComputedValue()
    {
        // P(0) + P(1) for lambda=3: 0.0497871 + 0.1493612 = 0.1991483
        PoissonDistribution.Cdf(1, 3).Should().BeApproximately(0.1991483, 1e-6);
    }
}
