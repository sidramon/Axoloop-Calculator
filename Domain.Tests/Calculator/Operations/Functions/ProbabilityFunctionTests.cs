namespace Domain.Tests.Calculator.Operations.Functions;

using Domain.Calculator.Operations.Functions.Probability;
using Domain.Calculator.Values;
using FluentAssertions;
using Value = Domain.Calculator.Values.Value;

public class ProbabilityFunctionTests
{
    private static MatrixValue Row(params double[] values)
    {
        var data = new double[1, values.Length];
        for (var i = 0; i < values.Length; i++) data[0, i] = values[i];
        return new MatrixValue(data);
    }

    private static MatrixValue Column(params double[] values)
    {
        var data = new double[values.Length, 1];
        for (var i = 0; i < values.Length; i++) data[i, 0] = values[i];
        return new MatrixValue(data);
    }

    private static readonly MatrixValue EmptyVector = new(new double[1, 0]);

    // ---- Descriptive statistics: wiring, both vector orientations, empty/non-vector errors ----

    [Fact]
    public void Mean_RowVector_ReturnsAverage()
    {
        var result = (NumberValue)new MeanFunction().Apply(new Value[] { Row(1, 2, 3, 4) });

        result.Number.Should().BeApproximately(2.5, 1e-10);
    }

    [Fact]
    public void Mean_ColumnVector_ReturnsAverage()
    {
        var result = (NumberValue)new MeanFunction().Apply(new Value[] { Column(1, 2, 3, 4) });

        result.Number.Should().BeApproximately(2.5, 1e-10);
    }

    [Fact]
    public void Mean_EmptyVector_Throws()
    {
        var act = () => new MeanFunction().Apply(new Value[] { EmptyVector });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Mean_NonVectorMatrix_Throws()
    {
        var matrix = new MatrixValue(new double[,] { { 1, 2 }, { 3, 4 } });

        var act = () => new MeanFunction().Apply(new Value[] { matrix });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Mean_NonMatrixArgument_Throws()
    {
        var act = () => new MeanFunction().Apply(new Value[] { new NumberValue(5) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Median_EvenLength_ReturnsAverageOfMiddleTwo()
    {
        var result = (NumberValue)new MedianFunction().Apply(new Value[] { Row(1, 2, 3, 4) });

        result.Number.Should().BeApproximately(2.5, 1e-10);
    }

    [Fact]
    public void Median_OddLength_ReturnsMiddleElement()
    {
        var result = (NumberValue)new MedianFunction().Apply(new Value[] { Row(1, 3, 2) });

        result.Number.Should().BeApproximately(2, 1e-10);
    }

    [Fact]
    public void Mode_KnownVector_ReturnsMostFrequentValue()
    {
        var result = (NumberValue)new ModeFunction().Apply(new Value[] { Row(1, 2, 2, 3) });

        result.Number.Should().BeApproximately(2, 1e-10);
    }

    [Fact]
    public void Variance_ArityOne_DefaultsToSampleConvention()
    {
        var result = (NumberValue)new VarianceFunction().Apply(new Value[] { Row(2, 4, 4, 4, 5, 5, 7, 9) });

        result.Number.Should().BeApproximately(32.0 / 7.0, 1e-9);
    }

    [Fact]
    public void Variance_ArityTwoWithNonzeroFlag_UsesPopulationConvention()
    {
        var result = (NumberValue)new VarianceFunction(hasPopulationArgument: true)
            .Apply(new Value[] { Row(2, 4, 4, 4, 5, 5, 7, 9), new NumberValue(1) });

        result.Number.Should().BeApproximately(4, 1e-9);
    }

    [Fact]
    public void Variance_ArityTwoWithZeroFlag_StillUsesSampleConvention()
    {
        var result = (NumberValue)new VarianceFunction(hasPopulationArgument: true)
            .Apply(new Value[] { Row(2, 4, 4, 4, 5, 5, 7, 9), new NumberValue(0) });

        result.Number.Should().BeApproximately(32.0 / 7.0, 1e-9);
    }

    [Fact]
    public void StdDev_ArityOne_DefaultsToSampleConvention()
    {
        var result = (NumberValue)new StdDevFunction().Apply(new Value[] { Row(2, 4, 4, 4, 5, 5, 7, 9) });

        result.Number.Should().BeApproximately(Math.Sqrt(32.0 / 7.0), 1e-9);
    }

    [Fact]
    public void StdDev_ArityTwoWithNonzeroFlag_UsesPopulationConvention()
    {
        var result = (NumberValue)new StdDevFunction(hasPopulationArgument: true)
            .Apply(new Value[] { Row(2, 4, 4, 4, 5, 5, 7, 9), new NumberValue(1) });

        result.Number.Should().BeApproximately(2, 1e-9);
    }

    [Fact]
    public void Range_KnownVector_ReturnsMaxMinusMin()
    {
        var result = (NumberValue)new RangeFunction().Apply(new Value[] { Row(3, 1, 4, 1, 5) });

        result.Number.Should().BeApproximately(4, 1e-10);
    }

    [Fact]
    public void Sum_KnownVector_ReturnsTotal()
    {
        var result = (NumberValue)new SumFunction().Apply(new Value[] { Row(1, 2, 3, 4) });

        result.Number.Should().BeApproximately(10, 1e-10);
    }

    [Fact]
    public void Product_KnownVector_ReturnsProduct()
    {
        var result = (NumberValue)new ProductFunction().Apply(new Value[] { Row(1, 2, 3, 4) });

        result.Number.Should().BeApproximately(24, 1e-10);
    }

    [Fact]
    public void Min_KnownVector_ReturnsSmallest()
    {
        var result = (NumberValue)new MinFunction().Apply(new Value[] { Row(3, 1, 4, 1, 5) });

        result.Number.Should().BeApproximately(1, 1e-10);
    }

    [Fact]
    public void Max_KnownVector_ReturnsLargest()
    {
        var result = (NumberValue)new MaxFunction().Apply(new Value[] { Row(3, 1, 4, 1, 5) });

        result.Number.Should().BeApproximately(5, 1e-10);
    }

    // ---- Combinatorics ----

    [Fact]
    public void Choose_KnownValues_ReturnsBinomialCoefficient()
    {
        var result = (NumberValue)new ChooseFunction().Apply(new Value[] { new NumberValue(52), new NumberValue(5) });

        result.Number.Should().Be(2598960);
    }

    [Fact]
    public void Choose_NonIntegerArgument_Throws()
    {
        var act = () => new ChooseFunction().Apply(new Value[] { new NumberValue(5.5), new NumberValue(2) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Permutations_KnownValues_ReturnsPermutationCount()
    {
        var result = (NumberValue)new PermutationsFunction().Apply(new Value[] { new NumberValue(10), new NumberValue(3) });

        result.Number.Should().Be(720);
    }

    // ---- Special functions ----

    [Fact]
    public void Erf_Zero_ReturnsZero()
    {
        var result = (NumberValue)new ErfFunction().Apply(new Value[] { new NumberValue(0) });

        result.Number.Should().BeApproximately(0, 1e-9);
    }

    [Fact]
    public void Erfc_Zero_ReturnsOne()
    {
        var result = (NumberValue)new ErfcFunction().Apply(new Value[] { new NumberValue(0) });

        result.Number.Should().BeApproximately(1, 1e-9);
    }

    [Fact]
    public void Gamma_Five_ReturnsTwentyFour()
    {
        var result = (NumberValue)new GammaFunction().Apply(new Value[] { new NumberValue(5) });

        result.Number.Should().BeApproximately(24, 1e-9);
    }

    [Fact]
    public void Gamma_NonPositiveInteger_Throws()
    {
        var act = () => new GammaFunction().Apply(new Value[] { new NumberValue(0) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LogGamma_Five_ReturnsLogTwentyFour()
    {
        var result = (NumberValue)new LogGammaFunction().Apply(new Value[] { new NumberValue(5) });

        result.Number.Should().BeApproximately(Math.Log(24), 1e-9);
    }

    // ---- Normal distribution ----

    [Fact]
    public void NormalPdf_StandardNormalAtZero_ReturnsPeakDensity()
    {
        var result = (NumberValue)new NormalPdfFunction().Apply(
            new Value[] { new NumberValue(0), new NumberValue(0), new NumberValue(1) });

        result.Number.Should().BeApproximately(1 / Math.Sqrt(2 * Math.PI), 1e-9);
    }

    [Fact]
    public void NormalPdf_NonPositiveSigma_Throws()
    {
        var act = () => new NormalPdfFunction().Apply(
            new Value[] { new NumberValue(0), new NumberValue(0), new NumberValue(-1) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NormalCdf_AtTheMean_ReturnsOneHalf()
    {
        var result = (NumberValue)new NormalCdfFunction().Apply(
            new Value[] { new NumberValue(0), new NumberValue(0), new NumberValue(1) });

        result.Number.Should().BeApproximately(0.5, 1e-6);
    }

    [Fact]
    public void NormalCdf_NonPositiveSigma_Throws()
    {
        var act = () => new NormalCdfFunction().Apply(
            new Value[] { new NumberValue(0), new NumberValue(0), new NumberValue(0) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void NormalInverseCdf_RoundTripsThroughNormalCdf()
    {
        var cdf = (NumberValue)new NormalCdfFunction().Apply(
            new Value[] { new NumberValue(1.5), new NumberValue(0), new NumberValue(1) });

        var result = (NumberValue)new NormalInverseCdfFunction().Apply(
            new Value[] { cdf, new NumberValue(0), new NumberValue(1) });

        result.Number.Should().BeApproximately(1.5, 1e-6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void NormalInverseCdf_POutsideOpenUnitInterval_Throws(double p)
    {
        var act = () => new NormalInverseCdfFunction().Apply(
            new Value[] { new NumberValue(p), new NumberValue(0), new NumberValue(1) });

        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Binomial distribution ----

    [Fact]
    public void BinomialPdf_KnownCase_ReturnsHandComputedProbability()
    {
        var result = (NumberValue)new BinomialPdfFunction().Apply(
            new Value[] { new NumberValue(2), new NumberValue(4), new NumberValue(0.5) });

        result.Number.Should().BeApproximately(0.375, 1e-9);
    }

    [Fact]
    public void BinomialPdf_POutOfRange_Throws()
    {
        var act = () => new BinomialPdfFunction().Apply(
            new Value[] { new NumberValue(2), new NumberValue(4), new NumberValue(1.5) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BinomialCdf_SumUpToK_ReturnsHandComputedProbability()
    {
        var result = (NumberValue)new BinomialCdfFunction().Apply(
            new Value[] { new NumberValue(2), new NumberValue(4), new NumberValue(0.5) });

        result.Number.Should().BeApproximately(0.6875, 1e-9);
    }

    // ---- Poisson distribution ----

    [Fact]
    public void PoissonPdf_KnownCase_ReturnsHandComputedProbability()
    {
        var result = (NumberValue)new PoissonPdfFunction().Apply(
            new Value[] { new NumberValue(0), new NumberValue(3) });

        result.Number.Should().BeApproximately(Math.Exp(-3), 1e-6);
    }

    [Fact]
    public void PoissonPdf_NonPositiveLambda_Throws()
    {
        var act = () => new PoissonPdfFunction().Apply(
            new Value[] { new NumberValue(1), new NumberValue(0) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PoissonCdf_SumUpToK_MatchesSumOfPmfs()
    {
        var pmf0 = (NumberValue)new PoissonPdfFunction().Apply(new Value[] { new NumberValue(0), new NumberValue(3) });
        var pmf1 = (NumberValue)new PoissonPdfFunction().Apply(new Value[] { new NumberValue(1), new NumberValue(3) });

        var result = (NumberValue)new PoissonCdfFunction().Apply(
            new Value[] { new NumberValue(1), new NumberValue(3) });

        result.Number.Should().BeApproximately(pmf0.Number + pmf1.Number, 1e-9);
    }
}
