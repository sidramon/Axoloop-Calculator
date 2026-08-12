namespace Domain.Tests.Calculator.Algorithms;

using Domain.Calculator.Algorithms;
using FluentAssertions;

public class DescriptiveStatisticsTests
{
    // Classic worked example (Wikipedia's "variance" article): mean 5, sum of squared
    // deviations 32, so sample variance = 32/7 and population variance = 32/8 = 4 exactly.
    private static readonly double[] Sample = { 2, 4, 4, 4, 5, 5, 7, 9 };

    [Fact]
    public void Mean_KnownSample_ReturnsFive()
    {
        DescriptiveStatistics.Mean(Sample).Should().BeApproximately(5, 1e-10);
    }

    [Fact]
    public void Mean_EmptyVector_Throws()
    {
        var act = () => DescriptiveStatistics.Mean(Array.Empty<double>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Median_OddLength_ReturnsMiddleElementAfterSorting()
    {
        DescriptiveStatistics.Median(new double[] { 1, 3, 2 }).Should().Be(2);
    }

    [Fact]
    public void Median_EvenLength_ReturnsAverageOfTwoMiddleElements()
    {
        DescriptiveStatistics.Median(new double[] { 1, 2, 3, 4 }).Should().Be(2.5);
    }

    [Fact]
    public void Median_UnsortedEvenLength_SortsBeforeAveraging()
    {
        DescriptiveStatistics.Median(new double[] { 9, 1, 3, 7 }).Should().Be(5);
    }

    [Fact]
    public void Median_EmptyVector_Throws()
    {
        var act = () => DescriptiveStatistics.Median(Array.Empty<double>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Mode_SingleMostFrequentValue_ReturnsIt()
    {
        DescriptiveStatistics.Mode(new double[] { 1, 2, 2, 3 }).Should().Be(2);
    }

    [Fact]
    public void Mode_TiedFrequencies_ReturnsSmallestTiedValue()
    {
        DescriptiveStatistics.Mode(new double[] { 1, 1, 2, 2 }).Should().Be(1);
    }

    [Fact]
    public void Mode_EmptyVector_Throws()
    {
        var act = () => DescriptiveStatistics.Mode(Array.Empty<double>());

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Variance: sample (n-1) vs population (n), verified by hand on the same vector ----

    [Fact]
    public void Variance_SampleConvention_DividesByNMinusOne()
    {
        // sum of squared deviations = 32, n - 1 = 7 -> 32/7
        DescriptiveStatistics.Variance(Sample).Should().BeApproximately(32.0 / 7.0, 1e-9);
    }

    [Fact]
    public void Variance_PopulationConvention_DividesByN()
    {
        // sum of squared deviations = 32, n = 8 -> 32/8 = 4 exactly
        DescriptiveStatistics.Variance(Sample, population: true).Should().BeApproximately(4, 1e-10);
    }

    [Fact]
    public void Variance_SampleAndPopulation_DifferByFactorOfNOverNMinusOne()
    {
        var sample = DescriptiveStatistics.Variance(Sample);
        var population = DescriptiveStatistics.Variance(Sample, population: true);

        (sample / population).Should().BeApproximately(8.0 / 7.0, 1e-9);
    }

    [Fact]
    public void Variance_SampleConventionWithSingleValue_Throws()
    {
        // n - 1 = 0 would divide by zero.
        var act = () => DescriptiveStatistics.Variance(new double[] { 5 });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Variance_PopulationConventionWithSingleValue_ReturnsZero()
    {
        DescriptiveStatistics.Variance(new double[] { 5 }, population: true).Should().Be(0);
    }

    [Fact]
    public void Variance_EmptyVector_Throws()
    {
        var act = () => DescriptiveStatistics.Variance(Array.Empty<double>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void StandardDeviation_SampleConvention_IsSquareRootOfSampleVariance()
    {
        DescriptiveStatistics.StandardDeviation(Sample).Should().BeApproximately(Math.Sqrt(32.0 / 7.0), 1e-9);
    }

    [Fact]
    public void StandardDeviation_PopulationConvention_IsExactlyTwo()
    {
        DescriptiveStatistics.StandardDeviation(Sample, population: true).Should().BeApproximately(2, 1e-10);
    }

    [Fact]
    public void Range_KnownSample_ReturnsMaxMinusMin()
    {
        DescriptiveStatistics.Range(new double[] { 3, 1, 4, 1, 5 }).Should().Be(4);
    }

    [Fact]
    public void Range_EmptyVector_Throws()
    {
        var act = () => DescriptiveStatistics.Range(Array.Empty<double>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Sum_KnownSample_ReturnsTotal()
    {
        DescriptiveStatistics.Sum(new double[] { 1, 2, 3, 4 }).Should().Be(10);
    }

    [Fact]
    public void Product_KnownSample_ReturnsProduct()
    {
        DescriptiveStatistics.Product(new double[] { 1, 2, 3, 4 }).Should().Be(24);
    }

    [Fact]
    public void Min_KnownSample_ReturnsSmallest()
    {
        DescriptiveStatistics.Min(new double[] { 3, 1, 4, 1, 5 }).Should().Be(1);
    }

    [Fact]
    public void Max_KnownSample_ReturnsLargest()
    {
        DescriptiveStatistics.Max(new double[] { 3, 1, 4, 1, 5 }).Should().Be(5);
    }

    [Fact]
    public void Sum_EmptyVector_Throws()
    {
        var act = () => DescriptiveStatistics.Sum(Array.Empty<double>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Product_EmptyVector_Throws()
    {
        var act = () => DescriptiveStatistics.Product(Array.Empty<double>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Min_EmptyVector_Throws()
    {
        var act = () => DescriptiveStatistics.Min(Array.Empty<double>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Max_EmptyVector_Throws()
    {
        var act = () => DescriptiveStatistics.Max(Array.Empty<double>());

        act.Should().Throw<InvalidOperationException>();
    }
}
